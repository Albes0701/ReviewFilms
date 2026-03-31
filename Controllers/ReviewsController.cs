using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewFilms.Api.DTOs.Common;
using ReviewFilms.Api.DTOs.Reviews;
using ReviewFilms.Api.Interfaces;

namespace ReviewFilms.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class ReviewsController : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService, ICurrentUserService currentUserService)
    {
        _reviewService = reviewService;
        _currentUserService = currentUserService;
    }

    [HttpPost("ratings")]
    public async Task<ActionResult<ApiResponse<object>>> UpsertRatingAsync(
        [FromBody] RatingRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();
        await _reviewService.UpsertRatingAsync(request.MovieId, request.Score, userId, cancellationToken);

        return Ok(ApiResponse<object>.Ok("Rating saved."));
    }

    [HttpDelete("ratings/{movieId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteRatingAsync(
        [FromRoute] Guid movieId,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();
        await _reviewService.DeleteRatingAsync(movieId, userId, cancellationToken);

        return Ok(ApiResponse<object>.Ok("Rating deleted."));
    }

    [HttpPost("comments")]
    public async Task<ActionResult<ApiResponse<CommentResponse>>> CreateCommentAsync(
        [FromBody] CommentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();
        var comment = await _reviewService.CreateCommentAsync(request, userId, cancellationToken);

        return Ok(ApiResponse<CommentResponse>.Ok(comment, "Comment created."));
    }

    [HttpGet("movies/{movieId:guid}/comments")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CommentResponse>>>> GetCommentsAsync(
        [FromRoute] Guid movieId,
        CancellationToken cancellationToken)
    {
        var comments = await _reviewService.GetCommentsAsync(movieId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CommentResponse>>.Ok(comments, "Comments loaded."));
    }
}
