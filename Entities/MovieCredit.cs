using ReviewFilms.Api.Enums;

namespace ReviewFilms.Api.Entities;

public class MovieCredit
{
    public Guid Id { get; set; }

    public Guid MovieId { get; set; }

    public Guid PersonId { get; set; }

    public CreditType CreditType { get; set; }

    public string? Department { get; set; }

    public string? Job { get; set; }

    public string? CharacterName { get; set; }

    public int? BillingOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public Movie Movie { get; set; } = null!;

    public Person Person { get; set; } = null!;
}
