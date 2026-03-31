using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReviewFilms.Api.Data;
using ReviewFilms.Api.DTOs.Reviews;
using ReviewFilms.Api.Entities;
using ReviewFilms.Api.Enums;
using ReviewFilms.Api.Interfaces;
using ReviewFilms.Api.Services;
using Xunit;

namespace ReviewFilms.Tests;

public sealed class ReviewServiceTests
{
    [Fact]
    public async Task CreateCommentAsync_creates_notification_when_replying_to_another_users_comment()
    {
        await using var dbContext = CreateDbContext();
        var movieId = Guid.NewGuid();
        var parentAuthorId = Guid.NewGuid();
        var replyAuthorId = Guid.NewGuid();
        var parentCommentId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        SeedUsersAndMovie(dbContext, movieId, now, parentAuthorId, replyAuthorId);
        dbContext.Comments.Add(new Comment
        {
            Id = parentCommentId,
            MovieId = movieId,
            UserId = parentAuthorId,
            Content = "Original comment",
            Depth = 0,
            Score = 0,
            UpvoteCount = 0,
            DownvoteCount = 0,
            ReplyCount = 0,
            IsEdited = false,
            Status = CommentStatus.Visible,
            CreatedAt = now,
            UpdatedAt = now,
            RootId = parentCommentId
        });
        await dbContext.SaveChangesAsync();

        var service = CreateServiceProvider(dbContext)
            .GetRequiredService<IReviewService>();

        var createdComment = await service.CreateCommentAsync(new CommentRequest
        {
            MovieId = movieId,
            ParentId = parentCommentId,
            Content = "Reply comment"
        }, replyAuthorId);

        var notification = await dbContext.Notifications.SingleAsync();
        using var payloadDocument = JsonDocument.Parse(notification.DataJson!);

        Assert.Equal(parentAuthorId, notification.UserId);
        Assert.Equal(NotificationType.CommentReply, notification.Type);
        Assert.Equal("Có người vừa trả lời bình luận của bạn", notification.Title);
        Assert.Equal(movieId.ToString(), payloadDocument.RootElement.GetProperty("movieId").GetString());
        Assert.Equal(createdComment.Id.ToString(), payloadDocument.RootElement.GetProperty("commentId").GetString());
    }

    [Fact]
    public async Task CreateCommentAsync_does_not_create_notification_for_self_reply()
    {
        await using var dbContext = CreateDbContext();
        var movieId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var parentCommentId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        SeedUsersAndMovie(dbContext, movieId, now, authorId);
        dbContext.Comments.Add(new Comment
        {
            Id = parentCommentId,
            MovieId = movieId,
            UserId = authorId,
            Content = "Original comment",
            Depth = 0,
            Score = 0,
            UpvoteCount = 0,
            DownvoteCount = 0,
            ReplyCount = 0,
            IsEdited = false,
            Status = CommentStatus.Visible,
            CreatedAt = now,
            UpdatedAt = now,
            RootId = parentCommentId
        });
        await dbContext.SaveChangesAsync();

        var service = CreateServiceProvider(dbContext)
            .GetRequiredService<IReviewService>();

        await service.CreateCommentAsync(new CommentRequest
        {
            MovieId = movieId,
            ParentId = parentCommentId,
            Content = "Self reply"
        }, authorId);

        Assert.Empty(await dbContext.Notifications.ToListAsync());
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ServiceProvider CreateServiceProvider(ApplicationDbContext dbContext)
    {
        var services = new ServiceCollection();
        services.AddSingleton(dbContext);
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IReviewService, ReviewService>();

        return services.BuildServiceProvider();
    }

    private static void SeedUsersAndMovie(
        ApplicationDbContext dbContext,
        Guid movieId,
        DateTime now,
        params Guid[] userIds)
    {
        foreach (var userId in userIds.Distinct())
        {
            dbContext.Users.Add(new User
            {
                Id = userId,
                Username = $"user-{userId:N}",
                Email = $"{userId:N}@example.com",
                PasswordHash = "hash",
                DisplayName = $"User {userId:N}",
                Status = UserStatus.Active,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        dbContext.Movies.Add(new Movie
        {
            Id = movieId,
            Title = "Movie",
            Slug = $"movie-{movieId:N}",
            Status = MovieStatus.Published,
            RatingCount = 0,
            CommentCount = 0,
            CreatedAt = now,
            UpdatedAt = now
        });

        dbContext.SaveChanges();
    }
}
