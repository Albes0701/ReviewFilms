using System.IdentityModel.Tokens.Jwt;
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
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    private readonly JwtOptions _jwtOptions;
    private readonly PasswordHasher _passwordHasher;

    public AuthService(
        ApplicationDbContext dbContext,
        JwtTokenGenerator jwtTokenGenerator,
        PasswordHasher passwordHasher,
        IOptions<JwtOptions> jwtOptions)
    {
        _dbContext = dbContext;
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

        var roles = user.UserRoles
            .Select(userRole => userRole.Role.Code)
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var response = await IssueTokensAsync(user, roles, now, cancellationToken);

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

        var roles = user.UserRoles
            .Select(userRole => userRole.Role.Code)
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var rotatedResponse = await IssueTokensAsync(
            user,
            roles,
            now,
            cancellationToken,
            refreshToken);

        return rotatedResponse;
    }

    private async Task<AuthResponse> IssueTokensAsync(
        User user,
        IReadOnlyCollection<string> roles,
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
            Roles = roles.ToArray()
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
