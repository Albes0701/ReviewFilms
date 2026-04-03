namespace ReviewFilms.Api.DTOs.Films;

public sealed class TmdbImportResultDto
{
    public bool IsSuccess { get; init; }

    public string Message { get; init; } = string.Empty;

    public Guid? MovieId { get; init; }

    public string? MovieSlug { get; init; }
}
