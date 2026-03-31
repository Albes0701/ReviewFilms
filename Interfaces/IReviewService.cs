using ReviewFilms.Api.DTOs.Reviews;

namespace ReviewFilms.Api.Interfaces;

public interface IReviewService
{
    Task UpsertRatingAsync(Guid movieId, int score, Guid userId, CancellationToken cancellationToken = default);

    Task DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default);

    Task<CommentResponse> CreateCommentAsync(CommentRequest request, Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CommentResponse>> GetCommentsAsync(Guid movieId, CancellationToken cancellationToken = default);
}
