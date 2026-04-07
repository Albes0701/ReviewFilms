using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ReviewFilms.Api.Configurations;
using ReviewFilms.Api.Data;
using ReviewFilms.Api.Entities;
using ReviewFilms.Api.Enums;
using ReviewFilms.Api.Interfaces;
using ReviewFilms.Api.Security;
using ReviewFilms.Api.Services;
using Xunit;

namespace ReviewFilms.Tests;

public sealed class AuthServiceProfileTests
{
    [Fact]
    public async Task LogoutAsync_revokes_matching_refresh_token()
    {
        await using var dbContext = CreateDbContext();
        SeedUserWithRefreshToken(dbContext);
        var service = CreateService(dbContext);

        var logoutMethod = typeof(AuthService).GetMethod("LogoutAsync");

        Assert.NotNull(logoutMethod);

        await InvokeTaskAsync(service, logoutMethod!, TestData.RawRefreshToken, CancellationToken.None);

        var persistedToken = await dbContext.RefreshTokens.SingleAsync();

        Assert.NotNull(persistedToken.RevokedAt);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_returns_profile_with_roles_and_permissions()
    {
        await using var dbContext = CreateDbContext();
        var userId = SeedUserWithRoles(dbContext);
        var service = CreateService(dbContext);

        var getProfileMethod = typeof(AuthService).GetMethod("GetCurrentUserProfileAsync");

        Assert.NotNull(getProfileMethod);

        var profile = await InvokeTaskWithResultAsync(service, getProfileMethod!, userId, CancellationToken.None);
        var profileType = profile.GetType();

        Assert.Equal(userId, profileType.GetProperty("UserId")!.GetValue(profile));
        Assert.Equal("Demo User", profileType.GetProperty("DisplayName")!.GetValue(profile));
        Assert.Equal("I review films.", profileType.GetProperty("Bio")!.GetValue(profile));

        var roles = Assert.IsType<string[]>(profileType.GetProperty("Roles")!.GetValue(profile));
        Assert.Equal(["ADMIN", "USER"], roles.OrderBy(role => role).ToArray());

        var permissionsProperty = profileType.GetProperty("Permissions");

        Assert.NotNull(permissionsProperty);

        var permissions = Assert.IsType<string[]>(permissionsProperty!.GetValue(profile));
        Assert.Equal(
            ["genres:sync", "movies:create", "movies:update"],
            permissions.OrderBy(permission => permission).ToArray());
    }

    [Fact]
    public async Task UpdateCurrentUserProfileAsync_updates_display_name_bio_and_avatar()
    {
        await using var dbContext = CreateDbContext();
        var userId = SeedUserWithRoles(dbContext);
        var cloudinaryService = new StubCloudinaryService("https://cdn.example.com/users/avatars/profile.jpg");
        var service = CreateService(dbContext, cloudinaryService);

        var requestType = typeof(AuthService).Assembly.GetType("ReviewFilms.Api.DTOs.Auth.UpdateUserProfileRequest");
        var updateProfileMethod = typeof(AuthService).GetMethod("UpdateCurrentUserProfileAsync");

        Assert.NotNull(requestType);
        Assert.NotNull(updateProfileMethod);

        var request = Activator.CreateInstance(requestType!);
        requestType!.GetProperty("DisplayName")!.SetValue(request, "Updated Demo");
        requestType.GetProperty("Bio")!.SetValue(request, "Updated biography.");
        requestType.GetProperty("AvatarFile")!.SetValue(request, CreateAvatarFile());

        var profile = await InvokeTaskWithResultAsync(service, updateProfileMethod!, userId, request!, CancellationToken.None);
        var profileType = profile.GetType();
        var persistedUser = await dbContext.Users.SingleAsync(user => user.Id == userId);

        Assert.Equal("Updated Demo", persistedUser.DisplayName);
        Assert.Equal("Updated biography.", persistedUser.Bio);
        Assert.Equal("https://cdn.example.com/users/avatars/profile.jpg", persistedUser.AvatarUrl);
        Assert.Equal("Updated Demo", profileType.GetProperty("DisplayName")!.GetValue(profile));
        Assert.Equal("Updated biography.", profileType.GetProperty("Bio")!.GetValue(profile));
        Assert.Equal("https://cdn.example.com/users/avatars/profile.jpg", profileType.GetProperty("AvatarUrl")!.GetValue(profile));
        Assert.Equal("users/avatars", cloudinaryService.LastFolder);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static AuthService CreateService(
        ApplicationDbContext dbContext,
        ICloudinaryService? cloudinaryService = null)
    {
        var jwtOptions = Options.Create(new JwtOptions
        {
            SecretKey = TestData.SecretKey,
            Issuer = TestData.Issuer,
            Audience = TestData.Audience,
            AccessTokenMinutes = 60,
            RefreshTokenDays = 30
        });

        var constructor = typeof(AuthService).GetConstructors().Single();
        var arguments = constructor.GetParameters()
            .Select(parameter => ResolveConstructorArgument(parameter.ParameterType, dbContext, jwtOptions, cloudinaryService))
            .ToArray();

        return (AuthService)Activator.CreateInstance(typeof(AuthService), arguments)!;
    }

    private static object ResolveConstructorArgument(
        Type parameterType,
        ApplicationDbContext dbContext,
        IOptions<JwtOptions> jwtOptions,
        ICloudinaryService? cloudinaryService)
    {
        if (parameterType == typeof(ApplicationDbContext))
        {
            return dbContext;
        }

        if (parameterType == typeof(JwtTokenGenerator))
        {
            return new JwtTokenGenerator(jwtOptions);
        }

        if (parameterType == typeof(PasswordHasher))
        {
            return new PasswordHasher();
        }

        if (parameterType == typeof(IOptions<JwtOptions>))
        {
            return jwtOptions;
        }

        if (parameterType == typeof(ICloudinaryService))
        {
            return cloudinaryService ?? new StubCloudinaryService(null);
        }

        throw new InvalidOperationException($"Unsupported constructor dependency: {parameterType.Name}");
    }

    private static Guid SeedUserWithRoles(ApplicationDbContext dbContext)
    {
        var now = DateTime.UtcNow;
        var userId = Guid.NewGuid();
        var userRoleId = Guid.NewGuid();
        var adminRoleId = Guid.NewGuid();
        var createPermissionId = Guid.NewGuid();
        var updatePermissionId = Guid.NewGuid();
        var syncGenresPermissionId = Guid.NewGuid();

        dbContext.Roles.AddRange(
            new Role
            {
                Id = userRoleId,
                Code = "USER",
                Name = "User",
                IsSystem = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Role
            {
                Id = adminRoleId,
                Code = "ADMIN",
                Name = "Admin",
                IsSystem = true,
                CreatedAt = now,
                UpdatedAt = now
            });

        dbContext.Permissions.AddRange(
            new Permission
            {
                Id = createPermissionId,
                Code = "movies:create",
                Name = "Create movie",
                Module = "movies",
                CreatedAt = now,
                UpdatedAt = now
            },
            new Permission
            {
                Id = updatePermissionId,
                Code = "movies:update",
                Name = "Update movie",
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

        dbContext.Users.Add(new User
        {
            Id = userId,
            Username = "demo",
            Email = "demo@example.com",
            PasswordHash = new PasswordHasher().Hash("Password123!"),
            DisplayName = "Demo User",
            AvatarUrl = "https://cdn.example.com/users/avatars/original.jpg",
            Bio = "I review films.",
            Status = UserStatus.Active,
            EmailConfirmed = true,
            LastLoginAt = now,
            CreatedAt = now,
            UpdatedAt = now
        });

        dbContext.UserRoles.AddRange(
            new UserRole
            {
                UserId = userId,
                RoleId = userRoleId,
                AssignedAt = now
            },
            new UserRole
            {
                UserId = userId,
                RoleId = adminRoleId,
                AssignedAt = now
            });

        dbContext.RolePermissions.AddRange(
            new RolePermission
            {
                RoleId = userRoleId,
                PermissionId = createPermissionId,
                CreatedAt = now
            },
            new RolePermission
            {
                RoleId = adminRoleId,
                PermissionId = createPermissionId,
                CreatedAt = now
            },
            new RolePermission
            {
                RoleId = adminRoleId,
                PermissionId = updatePermissionId,
                CreatedAt = now
            },
            new RolePermission
            {
                RoleId = adminRoleId,
                PermissionId = syncGenresPermissionId,
                CreatedAt = now
            });

        dbContext.SaveChanges();

        return userId;
    }

    private static void SeedUserWithRefreshToken(ApplicationDbContext dbContext)
    {
        var now = DateTime.UtcNow;
        var roleId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        dbContext.Roles.Add(new Role
        {
            Id = roleId,
            Code = "USER",
            Name = "User",
            IsSystem = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        dbContext.Users.Add(new User
        {
            Id = userId,
            Username = "logout-user",
            Email = "logout@example.com",
            PasswordHash = new PasswordHasher().Hash("Password123!"),
            DisplayName = "Logout User",
            Status = UserStatus.Active,
            EmailConfirmed = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        dbContext.UserRoles.Add(new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = now
        });

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = new PasswordHasher().Hash(TestData.RawRefreshToken),
            JwtId = TestData.JwtId,
            ExpiresAt = now.AddDays(30),
            CreatedAt = now
        });

        dbContext.SaveChanges();
    }

    private static FormFile CreateAvatarFile()
    {
        var stream = new MemoryStream([1, 2, 3, 4]);
        return new FormFile(stream, 0, stream.Length, "avatarFile", "avatar.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
    }

    private static async Task InvokeTaskAsync(object instance, MethodInfo methodInfo, params object[] arguments)
    {
        var task = Assert.IsAssignableFrom<Task>(methodInfo.Invoke(instance, arguments));
        await task;
    }

    private static async Task<object> InvokeTaskWithResultAsync(object instance, MethodInfo methodInfo, params object[] arguments)
    {
        var task = Assert.IsAssignableFrom<Task>(methodInfo.Invoke(instance, arguments));
        await task;

        return task.GetType().GetProperty("Result")!.GetValue(task)!;
    }

    private sealed class StubCloudinaryService : ICloudinaryService
    {
        private readonly string? _uploadUrl;

        public StubCloudinaryService(string? uploadUrl)
        {
            _uploadUrl = uploadUrl;
        }

        public string? LastFolder { get; private set; }

        public Task<string?> UploadImageAsync(
            IFormFile? file,
            string? folder = null,
            CancellationToken cancellationToken = default)
        {
            LastFolder = folder;
            return Task.FromResult(_uploadUrl);
        }
    }

    private static class TestData
    {
        public const string SecretKey = "0123456789abcdef0123456789abcdef";
        public const string Issuer = "ReviewFilms.Api";
        public const string Audience = "ReviewFilms.Frontend";
        public const string JwtId = "logout-test-jti";
        public const string RawRefreshToken = "logout-refresh-token";
    }
}
