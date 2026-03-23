using ReviewFilms.Api.Enums;

namespace ReviewFilms.Api.Entities;

public class Comment
{
    public Guid Id { get; set; }

    public Guid MovieId { get; set; }

    public Guid UserId { get; set; }

    public Guid? ParentId { get; set; }

    public Guid? RootId { get; set; }

    public string Content { get; set; } = string.Empty;

    public int Depth { get; set; }

    public int Score { get; set; }

    public int UpvoteCount { get; set; }

    public int DownvoteCount { get; set; }

    public int ReplyCount { get; set; }

    public bool IsEdited { get; set; }

    public DateTime? EditedAt { get; set; }

    public CommentStatus Status { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Movie Movie { get; set; } = null!;

    public User User { get; set; } = null!;

    public Comment? ParentComment { get; set; }

    public ICollection<Comment> ChildComments { get; set; } = new List<Comment>();

    public Comment? RootComment { get; set; }

    public ICollection<Comment> ThreadComments { get; set; } = new List<Comment>();

    public ICollection<CommentVote> CommentVotes { get; set; } = new List<CommentVote>();
}
