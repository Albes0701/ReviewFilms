using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReviewFilms.Api.Extensions;
using ReviewFilms.Api.Interfaces;
using ReviewFilms.Api.Services;
using Xunit;

namespace ReviewFilms.Tests;

public sealed class CurrentUserServiceTests
{
    [Fact]
    public void GetCurrentUserId_returns_name_identifier_claim()
    {
        var userId = Guid.NewGuid();
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                    ],
                    authenticationType: "Test"))
            }
        };

        var service = new CurrentUserService(httpContextAccessor);

        var actualUserId = service.GetCurrentUserId();

        Assert.Equal(userId, actualUserId);
    }

    [Fact]
    public void AddApplicationDbContext_registers_all_modules_and_a_single_shared_current_user_service()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "server=localhost;database=reviewfilms;user=root;password=secret",
                ["Jwt:SecretKey"] = "0123456789abcdef0123456789abcdef",
                ["Jwt:Issuer"] = "ReviewFilms.Api",
                ["Jwt:Audience"] = "ReviewFilms.Frontend"
            })
            .Build();

        services.AddApplicationDbContext(configuration);

        var currentUserDescriptors = services
            .Where(descriptor => descriptor.ServiceType == typeof(ICurrentUserService))
            .ToArray();

        Assert.Single(currentUserDescriptors);
        Assert.Equal(typeof(CurrentUserService), currentUserDescriptors[0].ImplementationType);
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IAuthService));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IMovieService));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IReviewService));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(INotificationService));
    }
}
