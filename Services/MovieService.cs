using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
    private readonly ITmdbSyncService _tmdbSyncService;
    private readonly ILogger<MovieService> _logger;

    public MovieService(
        ApplicationDbContext dbContext,
        ICloudinaryService cloudinaryService,
        ICurrentUserService currentUserService,
        ITmdbSyncService tmdbSyncService,
        ILogger<MovieService> logger)
    {
        _dbContext = dbContext;
        _cloudinaryService = cloudinaryService;
        _currentUserService = currentUserService;
        _tmdbSyncService = tmdbSyncService;
        _logger = logger;
    }

    public async Task<PagedResult<MovieDto>> GetMoviesAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        Guid? genreId = null,
        MovieStatus? status = null,
        Guid? personId = null,
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

        if (personId.HasValue)
        {
            query = query.Where(movie =>
                movie.MovieCredits.Any(movieCredit => movieCredit.PersonId == personId.Value));
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

    public async Task<IReadOnlyCollection<GenreListItemDto>> GetGenresAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Genres
            .AsNoTracking()
            .OrderBy(genre => genre.Name)
            .Select(genre => new GenreListItemDto
            {
                Id = genre.Id,
                Name = genre.Name,
                Slug = genre.Slug
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<PersonListItemDto>> GetPersonsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Persons
            .AsNoTracking()
            .OrderBy(person => person.Name)
            .Select(person => new PersonListItemDto
            {
                Id = person.Id,
                Name = person.Name,
                OriginalName = person.OriginalName,
                KnownForDepartment = person.KnownForDepartment,
                ProfileUrl = person.ProfileUrl
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<int> SyncGenresAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var tmdbGenres = await _tmdbSyncService.FetchGenresAsync(cancellationToken);
        var importedCount = 0;

        foreach (var tmdbGenre in tmdbGenres)
        {
            if (string.IsNullOrWhiteSpace(tmdbGenre.Name))
            {
                continue;
            }

            var normalizedName = NormalizeName(tmdbGenre.Name);
            var slug = NormalizeSlug(tmdbGenre.Name);
            var exists = await _dbContext.Genres
                .AnyAsync(
                    genre => genre.Name.ToLower() == normalizedName || genre.Slug == slug,
                    cancellationToken);

            if (exists)
            {
                continue;
            }

            _dbContext.Genres.Add(new Genre
            {
                Id = Guid.NewGuid(),
                Name = tmdbGenre.Name.Trim(),
                Slug = slug,
                Description = null,
                CreatedAt = now,
                UpdatedAt = now
            });

            importedCount++;
        }

        if (importedCount > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return importedCount;
    }

    public async Task<TmdbImportResultDto> ImportMovieFromTmdbAsync(
        int tmdbId,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var now = DateTime.UtcNow;
        var tmdbMovie = await _tmdbSyncService.FetchMovieDetailsAsync(tmdbId, cancellationToken);

        if (string.IsNullOrWhiteSpace(tmdbMovie.Title))
        {
            throw new InvalidOperationException($"TMDB movie '{tmdbId}' does not contain a title.");
        }

        var slug = NormalizeSlug(tmdbMovie.Title);
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new InvalidOperationException($"TMDB movie '{tmdbId}' produced an empty slug.");
        }

        var movieExists = await _dbContext.Movies
            .AsNoTracking()
            .AnyAsync(movie => movie.Slug == slug, cancellationToken);

        if (movieExists)
        {
            return new TmdbImportResultDto
            {
                IsSuccess = false,
                Message = "Đã tồn tại",
                MovieSlug = slug
            };
        }

        await using var transaction = await BeginTransactionIfNeededAsync(cancellationToken);

        try
        {
            var genres = await EnsureGenresAsync(tmdbMovie.Genres, now, cancellationToken);
            var movie = new Movie
            {
                Id = Guid.NewGuid(),
                Title = tmdbMovie.Title.Trim(),
                OriginalTitle = tmdbMovie.OriginalTitle?.Trim(),
                Slug = slug,
                Overview = tmdbMovie.Overview?.Trim(),
                ReleaseDate = tmdbMovie.ReleaseDate,
                RuntimeMinutes = tmdbMovie.RuntimeMinutes,
                AgeRating = null,
                OriginalLanguage = tmdbMovie.OriginalLanguage?.Trim(),
                PosterUrl = tmdbMovie.PosterUrl,
                BackdropUrl = tmdbMovie.BackdropUrl,
                TrailerUrl = null,
                AvgRating = null,
                RatingCount = 0,
                CommentCount = 0,
                Status = MovieStatus.Published,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedByUserId = currentUserId
            };

            _dbContext.Movies.Add(movie);

            foreach (var genre in genres)
            {
                _dbContext.Add(new MovieGenre
                {
                    MovieId = movie.Id,
                    GenreId = genre.Id,
                    CreatedAt = now
                });
            }

            foreach (var castMember in tmdbMovie.Cast)
            {
                var person = await EnsurePersonAsync(castMember, now, cancellationToken);
                _dbContext.MovieCredits.Add(new MovieCredit
                {
                    Id = Guid.NewGuid(),
                    MovieId = movie.Id,
                    PersonId = person.Id,
                    CreditType = CreditType.Cast,
                    Department = castMember.Department ?? castMember.KnownForDepartment,
                    Job = castMember.Job,
                    CharacterName = castMember.CharacterName,
                    BillingOrder = castMember.BillingOrder,
                    CreatedAt = now
                });
            }

            foreach (var crewMember in tmdbMovie.Crew)
            {
                var person = await EnsurePersonAsync(crewMember, now, cancellationToken);
                _dbContext.MovieCredits.Add(new MovieCredit
                {
                    Id = Guid.NewGuid(),
                    MovieId = movie.Id,
                    PersonId = person.Id,
                    CreditType = CreditType.Crew,
                    Department = crewMember.Department ?? crewMember.KnownForDepartment,
                    Job = crewMember.Job,
                    CharacterName = crewMember.CharacterName,
                    BillingOrder = crewMember.BillingOrder,
                    CreatedAt = now
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            return new TmdbImportResultDto
            {
                IsSuccess = true,
                Message = "Thành công",
                MovieId = movie.Id,
                MovieSlug = movie.Slug
            };
        }
        catch
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }

            throw;
        }
    }

    public async Task<BulkImportResultDto> ImportBulkPopularMoviesAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        count = Math.Clamp(count, 1, 200);

        var requiredPages = (int)Math.Ceiling(count / 20d);
        var tmdbIds = new List<int>(count);

        for (var page = 1; page <= requiredPages && tmdbIds.Count < count; page++)
        {
            var pageIds = await _tmdbSyncService.FetchPopularMovieIdsAsync(page, cancellationToken);
            tmdbIds.AddRange(pageIds);
        }

        var reviewedCount = 0;
        var importedCount = 0;
        var skippedCount = 0;
        var failedCount = 0;

        foreach (var tmdbId in tmdbIds.Take(count))
        {
            cancellationToken.ThrowIfCancellationRequested();
            reviewedCount++;

            try
            {
                var result = await ImportMovieFromTmdbAsync(tmdbId, cancellationToken);
                if (result.IsSuccess)
                {
                    importedCount++;
                }
                else
                {
                    skippedCount++;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                failedCount++;
                _logger.LogWarning(
                    exception,
                    "Failed to import TMDB movie {TmdbId}. Continuing bulk import.",
                    tmdbId);
            }
        }

        var message = failedCount == 0
            ? $"Đã duyệt {reviewedCount} phim, Import thành công {importedCount} phim, Bỏ qua {skippedCount} phim trùng lặp."
            : $"Đã duyệt {reviewedCount} phim, Import thành công {importedCount} phim, Bỏ qua {skippedCount} phim trùng lặp, Lỗi {failedCount} phim.";

        return new BulkImportResultDto
        {
            RequestedCount = count,
            ReviewedCount = reviewedCount,
            ImportedCount = importedCount,
            SkippedCount = skippedCount,
            FailedCount = failedCount,
            Message = message
        };
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

    private async Task<List<Genre>> EnsureGenresAsync(
        IReadOnlyCollection<TmdbGenreDto> tmdbGenres,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var genreInputs = tmdbGenres
            .Where(genre => !string.IsNullOrWhiteSpace(genre.Name))
            .Select(genre => new
            {
                Name = genre.Name.Trim(),
                NormalizedName = NormalizeName(genre.Name),
                Slug = NormalizeSlug(genre.Name)
            })
            .DistinctBy(genre => genre.Slug)
            .ToArray();

        if (genreInputs.Length == 0)
        {
            return [];
        }

        var genres = await _dbContext.Genres
            .ToListAsync(cancellationToken);

        foreach (var genreInput in genreInputs)
        {
            var existingGenre = genres.FirstOrDefault(genre =>
                NormalizeName(genre.Name) == genreInput.NormalizedName ||
                genre.Slug == genreInput.Slug);

            if (existingGenre is not null)
            {
                continue;
            }

            var newGenre = new Genre
            {
                Id = Guid.NewGuid(),
                Name = genreInput.Name,
                Slug = genreInput.Slug,
                Description = null,
                CreatedAt = now,
                UpdatedAt = now
            };

            genres.Add(newGenre);
            _dbContext.Genres.Add(newGenre);
        }

        return genres;
    }

    private async Task<Person> EnsurePersonAsync(
        TmdbMovieCreditPersonDto tmdbPerson,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tmdbPerson.Name))
        {
            throw new InvalidOperationException("TMDB credit person is missing a name.");
        }

        var normalizedName = NormalizeName(tmdbPerson.Name);
        var localPerson = _dbContext.Persons.Local
            .FirstOrDefault(existingPerson => NormalizeName(existingPerson.Name) == normalizedName);

        if (localPerson is not null)
        {
            return localPerson;
        }

        var person = await _dbContext.Persons
            .FirstOrDefaultAsync(
                existingPerson => existingPerson.Name.ToLower() == normalizedName,
                cancellationToken);

        if (person is not null)
        {
            return person;
        }

        person = new Person
        {
            Id = Guid.NewGuid(),
            Name = tmdbPerson.Name.Trim(),
            OriginalName = tmdbPerson.OriginalName?.Trim(),
            KnownForDepartment = tmdbPerson.KnownForDepartment?.Trim(),
            Gender = null,
            Birthday = null,
            Deathday = null,
            PlaceOfBirth = null,
            Biography = null,
            ProfileUrl = tmdbPerson.ProfileUrl,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.Persons.Add(person);
        return person;
    }

    private async Task<IDbContextTransaction?> BeginTransactionIfNeededAsync(CancellationToken cancellationToken)
    {
        return _dbContext.Database.IsRelational()
            ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
            : null;
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

    private static string NormalizeName(string value)
    {
        return value.Trim().ToLowerInvariant();
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
