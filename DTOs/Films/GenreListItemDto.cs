namespace ReviewFilms.Api.DTOs.Films;

public sealed class GenreListItemDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;
}
