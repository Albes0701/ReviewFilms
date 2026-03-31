using System.ComponentModel.DataAnnotations;

namespace ReviewFilms.Api.DTOs.Reviews;

public sealed class CommentRequest
{
    [Required]
    public Guid MovieId { get; init; }

    [Required]
    [StringLength(4000, MinimumLength = 1)]
    public string Content { get; init; } = string.Empty;

    public Guid? ParentId { get; init; }
}
