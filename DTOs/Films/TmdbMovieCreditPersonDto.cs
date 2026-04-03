namespace ReviewFilms.Api.DTOs.Films;

public sealed class TmdbMovieCreditPersonDto
{
    public string Name { get; init; } = string.Empty;

    public string? OriginalName { get; init; }

    public string? KnownForDepartment { get; init; }

    public string? ProfileUrl { get; init; }

    public string? Department { get; init; }

    public string? Job { get; init; }

    public string? CharacterName { get; init; }

    public int? BillingOrder { get; init; }
}
