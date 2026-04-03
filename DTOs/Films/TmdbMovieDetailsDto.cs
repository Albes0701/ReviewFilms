namespace ReviewFilms.Api.DTOs.Films;

public sealed class TmdbMovieDetailsDto
{
    public string Title { get; init; } = string.Empty;

    public string? OriginalTitle { get; init; }

    public string? Overview { get; init; }

    public DateOnly? ReleaseDate { get; init; }

    public int? RuntimeMinutes { get; init; }

    public string? OriginalLanguage { get; init; }

    public string? PosterUrl { get; init; }

    public string? BackdropUrl { get; init; }

    public IReadOnlyCollection<TmdbGenreDto> Genres { get; init; } = [];

    public IReadOnlyCollection<TmdbMovieCreditPersonDto> Cast { get; init; } = [];

    public IReadOnlyCollection<TmdbMovieCreditPersonDto> Crew { get; init; } = [];
}
