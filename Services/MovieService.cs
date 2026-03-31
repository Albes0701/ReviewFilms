using Microsoft.EntityFrameworkCore;
using ReviewFilms.Api.Data;
using ReviewFilms.Api.DTOs.Common;
using ReviewFilms.Api.DTOs.Films;
using ReviewFilms.Api.Entities;
using ReviewFilms.Api.Enums;
using ReviewFilms.Api.Interfaces;

namespace ReviewFilms.Api.Services;

public sealed class MovieService : IMovieService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MovieService> _logger;

    public MovieService(
        ApplicationDbContext dbContext,
        ICloudinaryService cloudinaryService,
        ICurrentUserService currentUserService,
        ILogger<MovieService> logger)
    {
        _dbContext = dbContext;
        _cloudinaryService = cloudinaryService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<PagedResult<MovieDto>> GetMoviesAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        Guid? genreId = null,
        MovieStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _dbContext.Movies
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(movie =>
                movie.Title.Contains(normalizedSearch) ||
                (movie.OriginalTitle != null && movie.OriginalTitle.Contains(normalizedSearch)) ||
                movie.Slug.Contains(normalizedSearch));
        }

        if (genreId.HasValue)
        {
            query = query.Where(movie =>
                movie.MovieGenres.Any(movieGenre => movieGenre.GenreId == genreId.Value));
        }

        if (status.HasValue)
        {
            query = query.Where(movie => movie.Status == status.Value);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(movie => movie.ReleaseDate)
            .ThenByDescending(movie => movie.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(movie => new MovieDto
            {
                Id = movie.Id,
                Title = movie.Title,
                OriginalTitle = movie.OriginalTitle,
                Slug = movie.Slug,
                Overview = movie.Overview,
                ReleaseDate = movie.ReleaseDate,
                RuntimeMinutes = movie.RuntimeMinutes,
                AgeRating = movie.AgeRating,
                OriginalLanguage = movie.OriginalLanguage,
                PosterUrl = movie.PosterUrl,
                BackdropUrl = movie.BackdropUrl,
                TrailerUrl = movie.TrailerUrl,
                AvgRating = movie.AvgRating,
                RatingCount = movie.RatingCount,
                CommentCount = movie.CommentCount,
                Status = movie.Status,
                CreatedAt = movie.CreatedAt,
                UpdatedAt = movie.UpdatedAt,
                CreatedByUserId = movie.CreatedByUserId
            })
            .ToListAsync(cancellationToken);

        return PagedResult<MovieDto>.Create(items, pageNumber, pageSize, totalCount);
    }

    public async Task<MovieDto> GetMovieByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var movie = await _dbContext.Movies
            .AsNoTracking()
            .AsSplitQuery()
            .Include(movie => movie.MovieGenres)
                .ThenInclude(movieGenre => movieGenre.Genre)
            .Include(movie => movie.MovieCredits)
                .ThenInclude(movieCredit => movieCredit.Person)
            .FirstOrDefaultAsync(movie => movie.Id == id, cancellationToken);

        if (movie is null)
        {
            throw new KeyNotFoundException($"Movie with id '{id}' was not found.");
        }

        return MovieDto.FromEntity(movie, includeRelations: true);
    }

    public async Task<MovieDto> CreateMovieAsync(
        MovieCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var currentUserId = _currentUserService.GetCurrentUserId();
        var title = request.Title.Trim();
        var resolvedSlug = await GenerateUniqueSlugAsync(
            string.IsNullOrWhiteSpace(request.Slug) ? title : request.Slug.Trim(),
            null,
            cancellationToken);

        var genreIds = request.GenreIds.Distinct().ToArray();
        var genres = genreIds.Length == 0
            ? []
            : await LoadGenresAsync(genreIds, cancellationToken);

        var posterUrl = await _cloudinaryService.UploadImageAsync(request.PosterFile, "movies/posters", cancellationToken);
        var backdropUrl = await _cloudinaryService.UploadImageAsync(request.BackdropFile, "movies/backdrops", cancellationToken);

        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Title = title,
            OriginalTitle = request.OriginalTitle?.Trim(),
            Slug = resolvedSlug,
            Overview = request.Overview?.Trim(),
            ReleaseDate = request.ReleaseDate,
            RuntimeMinutes = request.RuntimeMinutes,
            AgeRating = request.AgeRating?.Trim(),
            OriginalLanguage = request.OriginalLanguage?.Trim(),
            PosterUrl = posterUrl,
            BackdropUrl = backdropUrl,
            TrailerUrl = request.TrailerUrl?.Trim(),
            AvgRating = null,
            RatingCount = 0,
            CommentCount = 0,
            Status = request.Status,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedByUserId = currentUserId,
            MovieGenres = genres.Select(genre => new MovieGenre
            {
                MovieId = Guid.Empty,
                GenreId = genre.Id,
                CreatedAt = now,
                Genre = genre
            }).ToList()
        };

        foreach (var movieGenre in movie.MovieGenres)
        {
            movieGenre.MovieId = movie.Id;
        }

        _dbContext.Movies.Add(movie);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetMovieByIdAsync(movie.Id, cancellationToken);
    }

    public async Task<MovieDto> UpdateMovieAsync(
        Guid id,
        MovieUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var movie = await _dbContext.Movies
            .Include(item => item.MovieGenres)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (movie is null)
        {
            throw new KeyNotFoundException($"Movie with id '{id}' was not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            movie.Title = request.Title.Trim();
        }

        if (request.OriginalTitle is not null)
        {
            movie.OriginalTitle = request.OriginalTitle.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            movie.Slug = await GenerateUniqueSlugAsync(request.Slug.Trim(), movie.Id, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.Title))
        {
            movie.Slug = await GenerateUniqueSlugAsync(movie.Title, movie.Id, cancellationToken);
        }

        if (request.Overview is not null)
        {
            movie.Overview = request.Overview.Trim();
        }

        if (request.ReleaseDate.HasValue)
        {
            movie.ReleaseDate = request.ReleaseDate;
        }

        if (request.RuntimeMinutes.HasValue)
        {
            movie.RuntimeMinutes = request.RuntimeMinutes;
        }

        if (request.AgeRating is not null)
        {
            movie.AgeRating = request.AgeRating.Trim();
        }

        if (request.OriginalLanguage is not null)
        {
            movie.OriginalLanguage = request.OriginalLanguage.Trim();
        }

        if (request.TrailerUrl is not null)
        {
            movie.TrailerUrl = request.TrailerUrl.Trim();
        }

        if (request.Status.HasValue)
        {
            movie.Status = request.Status.Value;
        }

        var posterUrl = await _cloudinaryService.UploadImageAsync(request.PosterFile, "movies/posters", cancellationToken);
        if (!string.IsNullOrWhiteSpace(posterUrl))
        {
            movie.PosterUrl = posterUrl;
        }

        var backdropUrl = await _cloudinaryService.UploadImageAsync(request.BackdropFile, "movies/backdrops", cancellationToken);
        if (!string.IsNullOrWhiteSpace(backdropUrl))
        {
            movie.BackdropUrl = backdropUrl;
        }

        if (request.GenreIds is not null)
        {
            var genreIds = request.GenreIds.Distinct().ToArray();
            var genres = genreIds.Length == 0
                ? []
                : await LoadGenresAsync(genreIds, cancellationToken);

            movie.MovieGenres.Clear();

            foreach (var genre in genres)
            {
                movie.MovieGenres.Add(new MovieGenre
                {
                    MovieId = movie.Id,
                    GenreId = genre.Id,
                    CreatedAt = DateTime.UtcNow,
                    Genre = genre
                });
            }
        }

        movie.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetMovieByIdAsync(movie.Id, cancellationToken);
    }

    private async Task<List<Genre>> LoadGenresAsync(Guid[] genreIds, CancellationToken cancellationToken)
    {
        var genres = await _dbContext.Genres
            .Where(genre => genreIds.Contains(genre.Id))
            .ToListAsync(cancellationToken);

        if (genres.Count != genreIds.Length)
        {
            var missingGenreIds = genreIds.Except(genres.Select(genre => genre.Id)).ToArray();
            throw new KeyNotFoundException(
                $"Genre ids not found: {string.Join(", ", missingGenreIds)}");
        }

        return genres;
    }

    private async Task<string> GenerateUniqueSlugAsync(
        string input,
        Guid? excludeMovieId,
        CancellationToken cancellationToken)
    {
        var baseSlug = NormalizeSlug(input);
        if (string.IsNullOrWhiteSpace(baseSlug))
        {
            throw new ArgumentException("Slug cannot be empty.", nameof(input));
        }

        var slugExists = await _dbContext.Movies
            .AsNoTracking()
            .Where(movie => !excludeMovieId.HasValue || movie.Id != excludeMovieId.Value)
            .AnyAsync(movie => movie.Slug == baseSlug, cancellationToken);

        if (!slugExists)
        {
            return baseSlug;
        }

        var existingSlugs = await _dbContext.Movies
            .AsNoTracking()
            .Where(movie => !excludeMovieId.HasValue || movie.Id != excludeMovieId.Value)
            .Select(movie => movie.Slug)
            .ToListAsync(cancellationToken);

        var prefix = baseSlug + "-";
        var nextSuffix = existingSlugs
            .Where(slug => slug == baseSlug || slug.StartsWith(prefix))
            .Select(slug => GetSlugSuffix(baseSlug, slug))
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .DefaultIfEmpty(1)
            .Max() + 1;

        return $"{baseSlug}-{nextSuffix}";
    }

    private static string NormalizeSlug(string value)
    {
        var slug = value.Trim().ToLowerInvariant();
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", string.Empty);
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[\s-]+", "-");
        return slug.Trim('-');
    }

    private static int? GetSlugSuffix(string baseSlug, string slug)
    {
        if (slug == baseSlug)
        {
            return 1;
        }

        var prefix = baseSlug + "-";
        if (!slug.StartsWith(prefix))
        {
            return null;
        }

        return int.TryParse(slug[prefix.Length..], out var suffix) ? suffix : null;
    }
}
