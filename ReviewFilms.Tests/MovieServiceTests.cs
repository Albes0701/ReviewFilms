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
    public async Task GetMoviesAsync_filters_movies_by_person_id()
    {
        await using var dbContext = CreateDbContext();
        var currentUserId = Guid.NewGuid();
        SeedUser(dbContext, currentUserId);

        var targetPersonId = Guid.NewGuid();
        var otherPersonId = Guid.NewGuid();
        var matchingMovieId = Guid.NewGuid();
        var otherMovieId = Guid.NewGuid();

        dbContext.Persons.AddRange(
            new Person
            {
                Id = targetPersonId,
                Name = "Christopher Nolan",
                KnownForDepartment = "Directing",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Person
            {
                Id = otherPersonId,
                Name = "Denis Villeneuve",
                KnownForDepartment = "Directing",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

        dbContext.Movies.AddRange(
            new Movie
            {
                Id = matchingMovieId,
                Title = "Interstellar",
                Slug = "interstellar",
                Status = MovieStatus.Published,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedByUserId = currentUserId
            },
            new Movie
            {
                Id = otherMovieId,
                Title = "Arrival",
                Slug = "arrival",
                Status = MovieStatus.Published,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedByUserId = currentUserId
            });

        dbContext.MovieCredits.AddRange(
            new MovieCredit
            {
                Id = Guid.NewGuid(),
                MovieId = matchingMovieId,
                PersonId = targetPersonId,
                CreditType = CreditType.Crew,
                Department = "Directing",
                Job = "Director",
                CreatedAt = DateTime.UtcNow
            },
            new MovieCredit
            {
                Id = Guid.NewGuid(),
                MovieId = otherMovieId,
                PersonId = otherPersonId,
                CreditType = CreditType.Crew,
                Department = "Directing",
                Job = "Director",
                CreatedAt = DateTime.UtcNow
            });

        await dbContext.SaveChangesAsync();

        var service = CreateServiceProvider(dbContext, currentUserId, new StubTmdbSyncService())
            .GetRequiredService<IMovieService>();

        var result = await service.GetMoviesAsync(1, 10, personId: targetPersonId);

        var movie = Assert.Single(result.Items);
        Assert.Equal(matchingMovieId, movie.Id);
        Assert.Equal("Interstellar", movie.Title);
    }

    [Fact]
    public async Task CreateMovieAsync_assigns_created_by_user_id_from_current_user_service()
    {
        await using var dbContext = CreateDbContext();
        var currentUserId = Guid.NewGuid();
        SeedUser(dbContext, currentUserId);
        await dbContext.SaveChangesAsync();

        var service = CreateServiceProvider(dbContext, currentUserId, new StubTmdbSyncService())
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

    [Fact]
    public async Task SyncGenresAsync_adds_only_missing_genres()
    {
        await using var dbContext = CreateDbContext();
        var currentUserId = Guid.NewGuid();
        SeedUser(dbContext, currentUserId);

        dbContext.Genres.Add(new Genre
        {
            Id = Guid.NewGuid(),
            Name = "Action",
            Slug = "action",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var tmdbSyncService = new StubTmdbSyncService();
        tmdbSyncService.Genres.AddRange(
            new TmdbGenreDto { Name = "Action" },
            new TmdbGenreDto { Name = "Comedy" });

        var service = CreateServiceProvider(dbContext, currentUserId, tmdbSyncService)
            .GetRequiredService<IMovieService>();

        var importedGenreCount = await service.SyncGenresAsync();

        var genres = await dbContext.Genres
            .OrderBy(genre => genre.Name)
            .ToListAsync();

        Assert.Equal(1, importedGenreCount);
        Assert.Equal(2, genres.Count);
        Assert.Equal(["Action", "Comedy"], genres.Select(genre => genre.Name).ToArray());
    }

    [Fact]
    public async Task ImportMovieFromTmdbAsync_returns_duplicate_result_when_slug_already_exists()
    {
        await using var dbContext = CreateDbContext();
        var currentUserId = Guid.NewGuid();
        SeedUser(dbContext, currentUserId);

        dbContext.Movies.Add(new Movie
        {
            Id = Guid.NewGuid(),
            Title = "The Matrix",
            Slug = "the-matrix",
            Status = MovieStatus.Published,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedByUserId = currentUserId
        });
        await dbContext.SaveChangesAsync();

        var tmdbSyncService = new StubTmdbSyncService();
        tmdbSyncService.MovieDetails[603] = new TmdbMovieDetailsDto
        {
            Title = "The Matrix"
        };

        var service = CreateServiceProvider(dbContext, currentUserId, tmdbSyncService)
            .GetRequiredService<IMovieService>();

        var result = await service.ImportMovieFromTmdbAsync(603);

        Assert.False(result.IsSuccess);
        Assert.Equal("Đã tồn tại", result.Message);
        Assert.Equal(1, await dbContext.Movies.CountAsync());
    }

    [Fact]
    public async Task ImportMovieFromTmdbAsync_reuses_existing_person_by_name()
    {
        await using var dbContext = CreateDbContext();
        var currentUserId = Guid.NewGuid();
        SeedUser(dbContext, currentUserId);

        var existingPersonId = Guid.NewGuid();
        dbContext.Persons.Add(new Person
        {
            Id = existingPersonId,
            Name = "Keanu Reeves",
            OriginalName = "Keanu Charles Reeves",
            KnownForDepartment = "Acting",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var tmdbSyncService = new StubTmdbSyncService();
        tmdbSyncService.MovieDetails[100] = new TmdbMovieDetailsDto
        {
            Title = "John Wick",
            Genres = [new TmdbGenreDto { Name = "Action" }],
            Cast =
            [
                new TmdbMovieCreditPersonDto
                {
                    Name = "Keanu Reeves",
                    OriginalName = "Keanu Charles Reeves",
                    KnownForDepartment = "Acting",
                    CharacterName = "John Wick",
                    BillingOrder = 0
                }
            ],
            Crew =
            [
                new TmdbMovieCreditPersonDto
                {
                    Name = "Chad Stahelski",
                    Department = "Directing",
                    Job = "Director"
                }
            ]
        };

        var service = CreateServiceProvider(dbContext, currentUserId, tmdbSyncService)
            .GetRequiredService<IMovieService>();

        var result = await service.ImportMovieFromTmdbAsync(100);

        var people = await dbContext.Persons
            .OrderBy(person => person.Name)
            .ToListAsync();
        var credit = await dbContext.MovieCredits
            .SingleAsync(movieCredit => movieCredit.PersonId == existingPersonId);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, people.Count);
        Assert.Equal(existingPersonId, credit.PersonId);
    }

    [Fact]
    public async Task ImportBulkPopularMoviesAsync_continues_when_one_movie_fails()
    {
        await using var dbContext = CreateDbContext();
        var currentUserId = Guid.NewGuid();
        SeedUser(dbContext, currentUserId);

        dbContext.Movies.Add(new Movie
        {
            Id = Guid.NewGuid(),
            Title = "Duplicate Movie",
            Slug = "duplicate-movie",
            Status = MovieStatus.Published,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedByUserId = currentUserId
        });
        await dbContext.SaveChangesAsync();

        var tmdbSyncService = new StubTmdbSyncService();
        tmdbSyncService.PopularMovieIds[1] = [1, 2, 3];
        tmdbSyncService.MovieDetails[1] = new TmdbMovieDetailsDto
        {
            Title = "Imported Movie",
            Genres = [new TmdbGenreDto { Name = "Sci-Fi" }]
        };
        tmdbSyncService.MovieDetails[3] = new TmdbMovieDetailsDto
        {
            Title = "Duplicate Movie"
        };
        tmdbSyncService.Exceptions[2] = new InvalidOperationException("TMDB payload is incomplete.");

        var service = CreateServiceProvider(dbContext, currentUserId, tmdbSyncService)
            .GetRequiredService<IMovieService>();

        var result = await service.ImportBulkPopularMoviesAsync(3);

        Assert.Equal(3, result.ReviewedCount);
        Assert.Equal(1, result.ImportedCount);
        Assert.Equal(1, result.SkippedCount);
        Assert.Equal(1, result.FailedCount);
        Assert.Equal(2, await dbContext.Movies.CountAsync());
    }

    [Fact]
    public void Genre_lookup_query_pattern_is_translatable_by_mysql_provider()
    {
        using var dbContext = CreateMySqlQueryContext();
        var normalizedNames = new[] { "action", "comedy" };
        var slugs = new[] { "action", "comedy" };

        var exception = Record.Exception(() =>
            dbContext.Genres
                .Where(genre =>
                    normalizedNames.Contains(genre.Name.ToLower()) ||
                    slugs.Contains(genre.Slug))
                .ToQueryString());

        Assert.NotNull(exception);
        Assert.Contains("type mapping assigned", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Movie_person_filter_query_pattern_translates_to_exists()
    {
        using var dbContext = CreateMySqlQueryContext();
        var personId = Guid.NewGuid();

        var sql = dbContext.Movies
            .AsNoTracking()
            .Where(movie => movie.MovieCredits.Any(movieCredit => movieCredit.PersonId == personId))
            .OrderByDescending(movie => movie.ReleaseDate)
            .ThenByDescending(movie => movie.CreatedAt)
            .Select(movie => movie.Id)
            .ToQueryString();

        Assert.Contains("EXISTS", sql, StringComparison.OrdinalIgnoreCase);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ApplicationDbContext CreateMySqlQueryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySQL("Server=localhost;Port=3306;Database=reviewfilms_test;User Id=root;Password=admin;Pooling=false")
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ServiceProvider CreateServiceProvider(
        ApplicationDbContext dbContext,
        Guid currentUserId,
        ITmdbSyncService tmdbSyncService)
    {
        var services = new ServiceCollection();
        services.AddSingleton(dbContext);
        services.AddSingleton<ICloudinaryService, StubCloudinaryService>();
        services.AddSingleton<ICurrentUserService>(new StubCurrentUserService(currentUserId));
        services.AddSingleton<ITmdbSyncService>(tmdbSyncService);
        services.AddLogging();
        services.AddScoped<IMovieService, MovieService>();

        return services.BuildServiceProvider();
    }

    private static void SeedUser(ApplicationDbContext dbContext, Guid currentUserId)
    {
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

    private sealed class StubTmdbSyncService : ITmdbSyncService
    {
        public List<TmdbGenreDto> Genres { get; } = [];

        public Dictionary<int, TmdbMovieDetailsDto> MovieDetails { get; } = [];

        public Dictionary<int, IReadOnlyCollection<int>> PopularMovieIds { get; } = [];

        public Dictionary<int, Exception> Exceptions { get; } = [];

        public Task<IReadOnlyCollection<TmdbGenreDto>> FetchGenresAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<TmdbGenreDto>>(Genres);
        }

        public Task<TmdbMovieDetailsDto> FetchMovieDetailsAsync(int tmdbId, CancellationToken cancellationToken = default)
        {
            if (Exceptions.TryGetValue(tmdbId, out var exception))
            {
                throw exception;
            }

            if (!MovieDetails.TryGetValue(tmdbId, out var movieDetails))
            {
                throw new KeyNotFoundException($"TMDB movie '{tmdbId}' was not configured.");
            }

            return Task.FromResult(movieDetails);
        }

        public Task<IReadOnlyCollection<int>> FetchPopularMovieIdsAsync(int page, CancellationToken cancellationToken = default)
        {
            if (!PopularMovieIds.TryGetValue(page, out var movieIds))
            {
                return Task.FromResult<IReadOnlyCollection<int>>([]);
            }

            return Task.FromResult(movieIds);
        }
    }
}
