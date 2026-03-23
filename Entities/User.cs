using ReviewFilms.Api.Enums;

namespace ReviewFilms.Api.Entities;

public class User
{
    public Guid Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    public UserStatus Status { get; set; }

    public bool EmailConfirmed { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public ICollection<UserRole> AssignedUserRoles { get; set; } = new List<UserRole>();

    public ICollection<RolePermission> CreatedRolePermissions { get; set; } = new List<RolePermission>();

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public ICollection<Movie> CreatedMovies { get; set; } = new List<Movie>();

    public ICollection<MovieRating> MovieRatings { get; set; } = new List<MovieRating>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public ICollection<CommentVote> CommentVotes { get; set; } = new List<CommentVote>();

    public ICollection<Watchlist> Watchlists { get; set; } = new List<Watchlist>();

    public ICollection<Report> ReportsFiled { get; set; } = new List<Report>();

    public ICollection<Report> ReportsReviewed { get; set; } = new List<Report>();

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
