namespace ReviewFilms.Api.DTOs.Films;

public sealed class PersonListItemDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? OriginalName { get; init; }

    public string? KnownForDepartment { get; init; }

    public string? ProfileUrl { get; init; }
}
