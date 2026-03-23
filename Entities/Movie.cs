using ReviewFilms.Api.Enums;

namespace ReviewFilms.Api.Entities;

public class Movie
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? OriginalTitle { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string? Overview { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public int? RuntimeMinutes { get; set; }

    public string? AgeRating { get; set; }

    public string? OriginalLanguage { get; set; }

    public string? PosterUrl { get; set; }

    public string? BackdropUrl { get; set; }

    public string? TrailerUrl { get; set; }

    public decimal? AvgRating { get; set; }

    public int RatingCount { get; set; }

    public int CommentCount { get; set; }

    public MovieStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public User? CreatedByUser { get; set; }

    public ICollection<MovieCredit> MovieCredits { get; set; } = new List<MovieCredit>();

    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();

    public ICollection<MovieRating> MovieRatings { get; set; } = new List<MovieRating>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public ICollection<Watchlist> Watchlists { get; set; } = new List<Watchlist>();
}
