using ReviewFilms.Api.DTOs.Films;

namespace ReviewFilms.Api.Interfaces;

public interface ITmdbSyncService
{
    Task<IReadOnlyCollection<TmdbGenreDto>> FetchGenresAsync(CancellationToken cancellationToken = default);

    Task<TmdbMovieDetailsDto> FetchMovieDetailsAsync(int tmdbId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<int>> FetchPopularMovieIdsAsync(int page, CancellationToken cancellationToken = default);
}
