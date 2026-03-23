using ReviewFilms.Api.Enums;

namespace ReviewFilms.Api.Entities;

public class CommentVote
{
    public Guid Id { get; set; }

    public Guid CommentId { get; set; }

    public Guid UserId { get; set; }

    public VoteType VoteType { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Comment Comment { get; set; } = null!;

    public User User { get; set; } = null!;
}
