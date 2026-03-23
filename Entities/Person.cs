namespace ReviewFilms.Api.Entities;

public class Person
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? OriginalName { get; set; }

    public string? KnownForDepartment { get; set; }

    public string? Gender { get; set; }

    public DateOnly? Birthday { get; set; }

    public DateOnly? Deathday { get; set; }

    public string? PlaceOfBirth { get; set; }

    public string? Biography { get; set; }

    public string? ProfileUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<MovieCredit> MovieCredits { get; set; } = new List<MovieCredit>();
}
