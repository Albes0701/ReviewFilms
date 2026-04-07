using ReviewFilms.Api.DTOs.Common;
using ReviewFilms.Api.DTOs.Films;
using ReviewFilms.Api.Enums;

namespace ReviewFilms.Api.Interfaces;

public interface IMovieService
{
    Task<PagedResult<MovieDto>> GetMoviesAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        Guid? genreId = null,
        MovieStatus? status = null,
        Guid? personId = null,
        CancellationToken cancellationToken = default);

    Task<MovieDto> GetMovieByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<MovieDto> CreateMovieAsync(MovieCreateRequest request, CancellationToken cancellationToken = default);

    Task<MovieDto> UpdateMovieAsync(Guid id, MovieUpdateRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<GenreListItemDto>> GetGenresAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PersonListItemDto>> GetPersonsAsync(CancellationToken cancellationToken = default);

    Task<int> SyncGenresAsync(CancellationToken cancellationToken = default);

    Task<TmdbImportResultDto> ImportMovieFromTmdbAsync(int tmdbId, CancellationToken cancellationToken = default);

    Task<BulkImportResultDto> ImportBulkPopularMoviesAsync(int count, CancellationToken cancellationToken = default);
}
