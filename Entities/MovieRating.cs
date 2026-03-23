namespace ReviewFilms.Api.Entities;

public class MovieRating
{
    public Guid Id { get; set; }

    public Guid MovieId { get; set; }

    public Guid UserId { get; set; }

    public int Score { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Movie Movie { get; set; } = null!;

    public User User { get; set; } = null!;
}
