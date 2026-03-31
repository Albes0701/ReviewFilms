using System.ComponentModel.DataAnnotations;

namespace ReviewFilms.Api.DTOs.Reviews;

public sealed class RatingRequest
{
    [Required]
    public Guid MovieId { get; init; }

    [Range(1, 10)]
    public int Score { get; init; }
}
