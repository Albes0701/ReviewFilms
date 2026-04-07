using System.IdentityModel.Tokens.Jwt;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ReviewFilms.Api.Configurations;
using ReviewFilms.Api.Data;
using ReviewFilms.Api.DTOs.Auth;
using ReviewFilms.Api.Entities;
using ReviewFilms.Api.Enums;
using ReviewFilms.Api.Interfaces;
using ReviewFilms.Api.Security;

namespace ReviewFilms.Api.Services;

public sealed class AuthService : IAuthService
{
    private const string DefaultRoleCode = "USER";

    private readonly ApplicationDbContext _dbContext;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    private readonly JwtOptions _jwtOptions;
    private readonly PasswordHasher _passwordHasher;

    public AuthService(
        ApplicationDbContext dbContext,
        ICloudinaryService cloudinaryService,
        JwtTokenGenerator jwtTokenGenerator,
        PasswordHasher passwordHasher,
        IOptions<JwtOptions> jwtOptions)
    {
        _dbContext = dbContext;
        _cloudinaryService = cloudinaryService;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var username = NormalizeUsername(request.Username);
        var email = NormalizeEmail(request.Email);
        var displayName = string.IsNullOrWhiteSpace(request.DisplayName)
            ? request.Username.Trim()
            : request.DisplayName.Trim();

        var existingUser = await _dbContext.Users
            .AnyAsync(user =>
                user.DeletedAt == null &&
                (user.Username == username || user.Email == email),
                cancellationToken);

        if (existingUser)
        {
            throw new InvalidOperationException("Username or email is already in use.");
        }

        var defaultRole = await _dbContext.Roles
            .Include(role => role.RolePermissions)
                .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(role => role.Code == DefaultRoleCode, cancellationToken);

        if (defaultRole is null)
        {
            throw new InvalidOperationException(
                $"Default role '{DefaultRoleCode}' was not found. Seed the role before allowing registration.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            DisplayName = displayName,
            AvatarUrl = null,
            Bio = null,
            Status = UserStatus.Active,
            EmailConfirmed = false,
            LastLoginAt = now,
            CreatedAt = now,
            UpdatedAt = now,
            DeletedAt = null
        };

        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = defaultRole.Id,
            AssignedByUserId = null,
            AssignedAt = now
        };

        _dbContext.Users.Add(user);
        _dbContext.UserRoles.Add(userRole);

        var response = await IssueTokensAsync(
            user,
            [defaultRole.Code],
            GetDistinctPermissions([defaultRole]),
            now,
            cancellationToken);

        return response;
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var identifier = NormalizeIdentifier(request.UsernameOrEmail);

        var user = await _dbContext.Users
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                    .ThenInclude(role => role.RolePermissions)
                        .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(user =>
                user.DeletedAt == null &&
                (user.Username == identifier || user.Email == identifier),
                cancellationToken);

        if (user is null || user.Status != UserStatus.Active)
        {
            throw new UnauthorizedAccessException("Invalid username/email or password.");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username/email or password.");
        }

        user.LastLoginAt = now;
        user.UpdatedAt = now;

        var roles = GetDistinctRoles(user);
        var permissions = GetDistinctPermissions(user);

        var response = await IssueTokensAsync(user, roles, permissions, now, cancellationToken);

        return response;
    }

    public async Task<AuthResponse> RefreshAsync(
        RefreshRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var jwtId = ExtractJwtId(request.AccessToken);

        var refreshToken = await _dbContext.RefreshTokens
            .Include(token => token.User)
                .ThenInclude(user => user.UserRoles)
                    .ThenInclude(userRole => userRole.Role)
                        .ThenInclude(role => role.RolePermissions)
                            .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(token =>
                token.JwtId == jwtId &&
                token.RevokedAt == null &&
                token.ExpiresAt > now &&
                token.User.DeletedAt == null,
                cancellationToken);

        if (refreshToken is null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        if (!_passwordHasher.Verify(request.RefreshToken, refreshToken.TokenHash))
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var user = refreshToken.User;

        if (user.Status != UserStatus.Active)
        {
            throw new UnauthorizedAccessException("Account is not active.");
        }

        var roles = GetDistinctRoles(user);
        var permissions = GetDistinctPermissions(user);

        var rotatedResponse = await IssueTokensAsync(
            user,
            roles,
            permissions,
            now,
            cancellationToken,
            refreshToken);

        return rotatedResponse;
    }

    public async Task LogoutAsync(
        string? refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var persistedRefreshTokens = await _dbContext.RefreshTokens
            .Where(token => token.RevokedAt == null)
            .OrderByDescending(token => token.CreatedAt)
            .ToListAsync(cancellationToken);

        var matchedRefreshToken = persistedRefreshTokens
            .FirstOrDefault(token => _passwordHasher.Verify(refreshToken, token.TokenHash));

        if (matchedRefreshToken is null)
        {
            return;
        }

        matchedRefreshToken.RevokedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserProfileDto> GetCurrentUserProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await GetUserWithRolesAsync(userId, cancellationToken);
        return MapToUserProfileDto(user);
    }

    public async Task<UserProfileDto> UpdateCurrentUserProfileAsync(
        Guid userId,
        UpdateUserProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await GetUserWithRolesAsync(userId, cancellationToken);

        if (request.DisplayName is not null)
        {
            if (string.IsNullOrWhiteSpace(request.DisplayName))
            {
                throw new ValidationException("DisplayName must not be empty.");
            }

            user.DisplayName = request.DisplayName.Trim();
        }

        if (request.Bio is not null)
        {
            user.Bio = string.IsNullOrWhiteSpace(request.Bio)
                ? null
                : request.Bio.Trim();
        }

        if (request.AvatarFile is { Length: > 0 })
        {
            var avatarUrl = await _cloudinaryService.UploadImageAsync(
                request.AvatarFile,
                "users/avatars",
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(avatarUrl))
            {
                user.AvatarUrl = avatarUrl;
            }
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToUserProfileDto(user);
    }

    private async Task<AuthResponse> IssueTokensAsync(
        User user,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions,
        DateTime now,
        CancellationToken cancellationToken,
        RefreshToken? currentRefreshToken = null)
    {
        var jwtId = Guid.NewGuid().ToString("N");
        var accessTokenExpiresAt = now.AddMinutes(_jwtOptions.AccessTokenMinutes);
        var refreshTokenExpiresAt = now.AddDays(_jwtOptions.RefreshTokenDays);
        var refreshTokenId = Guid.NewGuid();

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(
            user,
            roles,
            permissions,
            jwtId,
            accessTokenExpiresAt);

        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = refreshTokenId,
            UserId = user.Id,
            TokenHash = _passwordHasher.Hash(refreshToken),
            JwtId = jwtId,
            DeviceName = null,
            IpAddress = null,
            UserAgent = null,
            ExpiresAt = refreshTokenExpiresAt,
            RevokedAt = null,
            ReplacedByTokenId = null,
            CreatedAt = now
        });

        if (currentRefreshToken is not null)
        {
            currentRefreshToken.RevokedAt = now;
            currentRefreshToken.ReplacedByTokenId = refreshTokenId;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = accessTokenExpiresAt,
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            Roles = roles.ToArray(),
            Permissions = permissions.ToArray()
        };
    }

    private static string ExtractJwtId(string accessToken)
    {
        try
        {
            var token = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var jwtId = token.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrWhiteSpace(jwtId))
            {
                throw new UnauthorizedAccessException("Access token does not contain a token id.");
            }

            return jwtId;
        }
        catch (Exception exception) when (
            exception is ArgumentException or SecurityTokenException or FormatException)
        {
            throw new UnauthorizedAccessException("Invalid access token.", exception);
        }
    }

    private async Task<User> GetUserWithRolesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(currentUser => currentUser.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                    .ThenInclude(role => role.RolePermissions)
                        .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(
                currentUser => currentUser.Id == userId && currentUser.DeletedAt == null,
                cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException("User profile was not found.");
        }

        return user;
    }

    private static UserProfileDto MapToUserProfileDto(User user)
    {
        var roles = GetDistinctRoles(user);
        var permissions = GetDistinctPermissions(user);

        return new UserProfileDto
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            Status = user.Status,
            EmailConfirmed = user.EmailConfirmed,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            Roles = roles,
            Permissions = permissions
        };
    }

    private static string[] GetDistinctRoles(User user)
    {
        return user.UserRoles
            .Select(userRole => userRole.Role.Code)
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(role => role, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string[] GetDistinctPermissions(User user)
    {
        var roles = user.UserRoles
            .Select(userRole => userRole.Role);

        return GetDistinctPermissions(roles);
    }

    private static string[] GetDistinctPermissions(IEnumerable<Role> roles)
    {
        return roles
            .SelectMany(role => role.RolePermissions)
            .Select(rolePermission => rolePermission.Permission.Code)
            .Where(permission => !string.IsNullOrWhiteSpace(permission))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(permission => permission, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string NormalizeUsername(string username)
    {
        return username.Trim().ToLowerInvariant();
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static string NormalizeIdentifier(string identifier)
    {
        return identifier.Trim().ToLowerInvariant();
    }
}
