using ReviewFilms.Api.Enums;

namespace ReviewFilms.Api.DTOs.Films;

public sealed class MovieCreditDto
{
    public Guid Id { get; init; }

    public Guid PersonId { get; init; }

    public string PersonName { get; init; } = string.Empty;

    public string? PersonOriginalName { get; init; }

    public CreditType CreditType { get; init; }

    public string? Department { get; init; }

    public string? Job { get; init; }

    public string? CharacterName { get; init; }

    public int? BillingOrder { get; init; }
}
