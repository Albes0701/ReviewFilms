using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ReviewFilms.Api.Configurations;
using ReviewFilms.Api.Data;
using ReviewFilms.Api.DTOs.Auth;
using ReviewFilms.Api.Entities;
using ReviewFilms.Api.Enums;
using ReviewFilms.Api.Security;
using ReviewFilms.Api.Services;
using Xunit;

namespace ReviewFilms.Tests;

public sealed class AuthServiceRefreshTests
{
    [Fact]
    public async Task RefreshAsync_rotates_the_refresh_token_and_returns_new_tokens()
    {
        await using var dbContext = CreateDbContext();
        var accessToken = SeedUserWithRefreshToken(dbContext);

        var service = CreateService(dbContext);

        var response = await service.RefreshAsync(
            new RefreshRequest
            {
                AccessToken = accessToken,
                RefreshToken = TestData.RawRefreshToken
            },
            CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.False(string.IsNullOrWhiteSpace(response.RefreshToken));
        Assert.NotEqual(TestData.RawRefreshToken, response.RefreshToken);

        var permissionsProperty = typeof(AuthResponse).GetProperty("Permissions");

        Assert.NotNull(permissionsProperty);

        var permissions = Assert.IsType<string[]>(permissionsProperty!.GetValue(response));
        Assert.Equal(["genres:sync", "movies:import"], permissions.OrderBy(permission => permission).ToArray());

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.Token);
        Assert.Contains(jwt.Claims, claim => claim.Type == System.Security.Claims.ClaimTypes.Role && claim.Value == "USER");
        Assert.Equal(
            ["genres:sync", "movies:import"],
            jwt.Claims
                .Where(claim => claim.Type == "permissions")
                .Select(claim => claim.Value)
                .OrderBy(value => value)
                .ToArray());

        var tokens = await dbContext.RefreshTokens
            .OrderBy(token => token.CreatedAt)
            .ToListAsync();

        Assert.Equal(2, tokens.Count);
        Assert.NotNull(tokens[0].RevokedAt);
        Assert.Equal(tokens[1].Id, tokens[0].ReplacedByTokenId);
    }

    private static AuthService CreateService(ApplicationDbContext dbContext)
    {
        var jwtOptions = Options.Create(new JwtOptions
        {
            SecretKey = TestData.SecretKey,
            Issuer = TestData.Issuer,
            Audience = TestData.Audience,
            AccessTokenMinutes = 60,
            RefreshTokenDays = 30
        });

        return new AuthService(
            dbContext,
            new StubCloudinaryService(),
            new JwtTokenGenerator(jwtOptions),
            new PasswordHasher(),
            jwtOptions);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static string SeedUserWithRefreshToken(ApplicationDbContext dbContext)
    {
        var now = DateTime.UtcNow;
        var importPermissionId = Guid.NewGuid();
        var syncGenresPermissionId = Guid.NewGuid();

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Code = "USER",
            Name = "User",
            IsSystem = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "demo",
            Email = "demo@example.com",
            PasswordHash = new PasswordHasher().Hash("Password123!"),
            DisplayName = "Demo User",
            Status = UserStatus.Active,
            EmailConfirmed = false,
            LastLoginAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        var tokenGenerator = new JwtTokenGenerator(
            Options.Create(new JwtOptions
            {
                SecretKey = TestData.SecretKey,
                Issuer = TestData.Issuer,
                Audience = TestData.Audience
            }));
        var generateAccessTokenMethod = typeof(JwtTokenGenerator).GetMethod("GenerateAccessToken");

        Assert.NotNull(generateAccessTokenMethod);

        var accessToken = Assert.IsType<string>(generateAccessTokenMethod!.Invoke(
            tokenGenerator,
            [user, new[] { role.Code }, new[] { "movies:import", "genres:sync" }, TestData.JwtId, now.AddMinutes(60)]));

        dbContext.Roles.Add(role);
        dbContext.Permissions.AddRange(
            new Permission
            {
                Id = importPermissionId,
                Code = "movies:import",
                Name = "Import movie",
                Module = "movies",
                CreatedAt = now,
                UpdatedAt = now
            },
            new Permission
            {
                Id = syncGenresPermissionId,
                Code = "genres:sync",
                Name = "Sync genres",
                Module = "genres",
                CreatedAt = now,
                UpdatedAt = now
            });
        dbContext.Users.Add(user);
        dbContext.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            AssignedAt = now
        });
        dbContext.RolePermissions.AddRange(
            new RolePermission
            {
                RoleId = role.Id,
                PermissionId = importPermissionId,
                CreatedAt = now
            },
            new RolePermission
            {
                RoleId = role.Id,
                PermissionId = syncGenresPermissionId,
                CreatedAt = now
            });

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = new PasswordHasher().Hash(TestData.RawRefreshToken),
            JwtId = TestData.JwtId,
            ExpiresAt = now.AddDays(30),
            CreatedAt = now
        });

        dbContext.SaveChanges();

        return accessToken;
    }

    private static class TestData
    {
        public const string SecretKey = "0123456789abcdef0123456789abcdef";
        public const string Issuer = "ReviewFilms.Api";
        public const string Audience = "ReviewFilms.Frontend";
        public const string JwtId = "refresh-test-jti";
        public const string RawRefreshToken = "refresh-token-value";
    }

    private sealed class StubCloudinaryService : ReviewFilms.Api.Interfaces.ICloudinaryService
    {
        public Task<string?> UploadImageAsync(
            Microsoft.AspNetCore.Http.IFormFile? file,
            string? folder = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<string?>(null);
        }
    }
}
