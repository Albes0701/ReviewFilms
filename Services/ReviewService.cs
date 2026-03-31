using Microsoft.EntityFrameworkCore;
using ReviewFilms.Api.Data;
using ReviewFilms.Api.DTOs.Reviews;
using ReviewFilms.Api.Entities;
using ReviewFilms.Api.Enums;
using ReviewFilms.Api.Interfaces;

namespace ReviewFilms.Api.Services;

public sealed class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _dbContext;

    public ReviewService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task UpsertRatingAsync(Guid movieId, int score, Guid userId, CancellationToken cancellationToken = default)
    {
        ValidateScore(score);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var movie = await GetMovieAsync(movieId, cancellationToken);
        var rating = await _dbContext.MovieRatings
            .SingleOrDefaultAsync(x => x.MovieId == movieId && x.UserId == userId, cancellationToken);

        var now = DateTime.UtcNow;

        if (rating is null)
        {
            rating = new MovieRating
            {
                Id = Guid.NewGuid(),
                MovieId = movieId,
                UserId = userId,
                Score = score,
                CreatedAt = now,
                UpdatedAt = now
            };

            _dbContext.MovieRatings.Add(rating);
        }
        else
        {
            rating.Score = score;
            rating.UpdatedAt = now;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await RecalculateMovieRatingAsync(movie, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var movie = await GetMovieAsync(movieId, cancellationToken);
        var rating = await _dbContext.MovieRatings
            .SingleOrDefaultAsync(x => x.MovieId == movieId && x.UserId == userId, cancellationToken);

        if (rating is null)
        {
            throw new KeyNotFoundException($"Rating for movie '{movieId}' and user '{userId}' was not found.");
        }

        _dbContext.MovieRatings.Remove(rating);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await RecalculateMovieRatingAsync(movie, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<CommentResponse> CreateCommentAsync(CommentRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException("Comment content is required.");
        }

        var movie = await GetMovieAsync(request.MovieId, cancellationToken);
        var now = DateTime.UtcNow;
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            MovieId = request.MovieId,
            UserId = userId,
            Content = request.Content.Trim(),
            Depth = 0,
            Score = 0,
            UpvoteCount = 0,
            DownvoteCount = 0,
            ReplyCount = 0,
            IsEdited = false,
            EditedAt = null,
            Status = CommentStatus.Visible,
            DeletedAt = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        if (request.ParentId.HasValue)
        {
            var parentComment = await _dbContext.Comments
                .SingleOrDefaultAsync(x => x.Id == request.ParentId.Value && x.MovieId == request.MovieId, cancellationToken);

            if (parentComment is null)
            {
                throw new KeyNotFoundException($"Parent comment '{request.ParentId.Value}' was not found for movie '{request.MovieId}'.");
            }

            if (parentComment.Status != CommentStatus.Visible)
            {
                throw new InvalidOperationException("Parent comment is not available for replies.");
            }

            comment.ParentId = parentComment.Id;
            comment.RootId = parentComment.RootId ?? parentComment.Id;
            comment.Depth = parentComment.Depth + 1;

            parentComment.ReplyCount++;
            parentComment.UpdatedAt = now;

            if (comment.RootId.HasValue && comment.RootId.Value != parentComment.Id)
            {
                var rootComment = await _dbContext.Comments
                    .SingleOrDefaultAsync(x => x.Id == comment.RootId.Value && x.MovieId == request.MovieId, cancellationToken);

                if (rootComment is null)
                {
                    throw new KeyNotFoundException($"Root comment '{comment.RootId.Value}' was not found for movie '{request.MovieId}'.");
                }

                rootComment.ReplyCount++;
                rootComment.UpdatedAt = now;
            }
        }
        else
        {
            comment.RootId = comment.Id;
        }

        movie.CommentCount++;
        _dbContext.Comments.Add(comment);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapCommentResponse(comment);
    }

    public async Task<IReadOnlyList<CommentResponse>> GetCommentsAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        await GetMovieAsync(movieId, cancellationToken);

        var flatComments = await _dbContext.Comments
            .AsNoTracking()
            .Where(x => x.MovieId == movieId && x.Status == CommentStatus.Visible)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new CommentResponse
            {
                Id = x.Id,
                MovieId = x.MovieId,
                UserId = x.UserId,
                ParentId = x.ParentId,
                RootId = x.RootId,
                Content = x.Content,
                Depth = x.Depth,
                Score = x.Score,
                UpvoteCount = x.UpvoteCount,
                DownvoteCount = x.DownvoteCount,
                ReplyCount = x.ReplyCount,
                IsEdited = x.IsEdited,
                EditedAt = x.EditedAt,
                Status = x.Status,
                DeletedAt = x.DeletedAt,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                ChildComments = new List<CommentResponse>()
            })
            .ToListAsync(cancellationToken);

        return BuildCommentTree(flatComments);
    }

    private async Task<Movie> GetMovieAsync(Guid movieId, CancellationToken cancellationToken)
    {
        var movie = await _dbContext.Movies.FirstOrDefaultAsync(x => x.Id == movieId, cancellationToken);
        if (movie is null)
        {
            throw new KeyNotFoundException($"Movie '{movieId}' was not found.");
        }

        return movie;
    }

    private async Task RecalculateMovieRatingAsync(Movie movie, CancellationToken cancellationToken)
    {
        var ratingCount = await _dbContext.MovieRatings
            .CountAsync(x => x.MovieId == movie.Id, cancellationToken);

        movie.RatingCount = ratingCount;

        if (ratingCount == 0)
        {
            movie.AvgRating = null;
            return;
        }

        var average = await _dbContext.MovieRatings
            .Where(x => x.MovieId == movie.Id)
            .Select(x => (decimal)x.Score)
            .AverageAsync(cancellationToken);

        movie.AvgRating = Math.Round(average, 2, MidpointRounding.AwayFromZero);
    }

    private static void ValidateScore(int score)
    {
        if (score is < 1 or > 10)
        {
            throw new ArgumentException("Rating score must be between 1 and 10.");
        }
    }

    private static CommentResponse MapCommentResponse(Comment comment)
    {
        return new CommentResponse
        {
            Id = comment.Id,
            MovieId = comment.MovieId,
            UserId = comment.UserId,
            ParentId = comment.ParentId,
            RootId = comment.RootId,
            Content = comment.Content,
            Depth = comment.Depth,
            Score = comment.Score,
            UpvoteCount = comment.UpvoteCount,
            DownvoteCount = comment.DownvoteCount,
            ReplyCount = comment.ReplyCount,
            IsEdited = comment.IsEdited,
            EditedAt = comment.EditedAt,
            Status = comment.Status,
            DeletedAt = comment.DeletedAt,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            ChildComments = new List<CommentResponse>()
        };
    }

    private static IReadOnlyList<CommentResponse> BuildCommentTree(IReadOnlyList<CommentResponse> flatComments)
    {
        var lookup = flatComments.ToDictionary(comment => comment.Id);
        var roots = new List<CommentResponse>();

        foreach (var comment in flatComments)
        {
            comment.ChildComments = new List<CommentResponse>();
        }

        foreach (var comment in flatComments)
        {
            if (comment.ParentId.HasValue && lookup.TryGetValue(comment.ParentId.Value, out var parent))
            {
                parent.ChildComments.Add(comment);
                continue;
            }

            roots.Add(comment);
        }

        SortChildren(roots);
        return roots.OrderBy(x => x.CreatedAt).ToList();
    }

    private static void SortChildren(IEnumerable<CommentResponse> comments)
    {
        foreach (var comment in comments)
        {
            comment.ChildComments = comment.ChildComments
                .OrderBy(x => x.CreatedAt)
                .ToList();

            if (comment.ChildComments.Count > 0)
            {
                SortChildren(comment.ChildComments);
            }
        }
    }
}
