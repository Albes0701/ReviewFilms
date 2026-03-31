using ReviewFilms.Api.Enums;

namespace ReviewFilms.Api.DTOs.Reviews;

public sealed class CommentResponse
{
    public Guid Id { get; init; }

    public Guid MovieId { get; init; }

    public Guid UserId { get; init; }

    public Guid? ParentId { get; init; }

    public Guid? RootId { get; init; }

    public string Content { get; init; } = string.Empty;

    public int Depth { get; init; }

    public int Score { get; init; }

    public int UpvoteCount { get; init; }

    public int DownvoteCount { get; init; }

    public int ReplyCount { get; init; }

    public bool IsEdited { get; init; }

    public DateTime? EditedAt { get; init; }

    public CommentStatus Status { get; init; }

    public DateTime? DeletedAt { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }

    public List<CommentResponse> ChildComments { get; set; } = [];
}
