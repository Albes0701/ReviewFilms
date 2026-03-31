using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReviewFilms.Api.Data;
using ReviewFilms.Api.DTOs.Films;
using ReviewFilms.Api.Entities;
using ReviewFilms.Api.Enums;
using ReviewFilms.Api.Interfaces;
using ReviewFilms.Api.Services;
using Xunit;

namespace ReviewFilms.Tests;

public sealed class MovieServiceTests
{
    [Fact]
    public async Task CreateMovieAsync_assigns_created_by_user_id_from_current_user_service()
    {
        await using var dbContext = CreateDbContext();
        var currentUserId = Guid.NewGuid();

        dbContext.Users.Add(new User
        {
            Id = currentUserId,
            Username = "admin",
            Email = "admin@example.com",
            PasswordHash = "hash",
            DisplayName = "Admin",
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var service = CreateServiceProvider(dbContext, currentUserId)
            .GetRequiredService<IMovieService>();

        var movie = await service.CreateMovieAsync(new MovieCreateRequest
        {
            Title = "Interstellar",
            Status = MovieStatus.Published
        });

        var persistedMovie = await dbContext.Movies.SingleAsync();

        Assert.Equal(currentUserId, movie.CreatedByUserId);
        Assert.Equal(currentUserId, persistedMovie.CreatedByUserId);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ServiceProvider CreateServiceProvider(ApplicationDbContext dbContext, Guid currentUserId)
    {
        var services = new ServiceCollection();
        services.AddSingleton(dbContext);
        services.AddSingleton<ICloudinaryService, StubCloudinaryService>();
        services.AddSingleton<ICurrentUserService>(new StubCurrentUserService(currentUserId));
        services.AddLogging();
        services.AddScoped<IMovieService, MovieService>();

        return services.BuildServiceProvider();
    }

    private sealed class StubCurrentUserService : ICurrentUserService
    {
        private readonly Guid _currentUserId;

        public StubCurrentUserService(Guid currentUserId)
        {
            _currentUserId = currentUserId;
        }

        public Guid GetCurrentUserId() => _currentUserId;
    }

    private sealed class StubCloudinaryService : ICloudinaryService
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
