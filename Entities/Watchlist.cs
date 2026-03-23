using ReviewFilms.Api.Enums;

namespace ReviewFilms.Api.Entities;

public class Watchlist
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid MovieId { get; set; }

    public WatchlistStatus Status { get; set; }

    public int? Priority { get; set; }

    public string? Note { get; set; }

    public DateTime AddedAt { get; set; }

    public DateTime? WatchedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;

    public Movie Movie { get; set; } = null!;
}
