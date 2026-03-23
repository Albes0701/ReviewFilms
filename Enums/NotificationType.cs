using NpgsqlTypes;

namespace ReviewFilms.Api.Enums;

public enum NotificationType
{
    [PgName("SYSTEM")]
    System,

    [PgName("COMMENT_REPLY")]
    CommentReply,

    [PgName("COMMENT_UPVOTE")]
    CommentUpvote,

    [PgName("REPORT_RESULT")]
    ReportResult,

    [PgName("WATCHLIST_REMINDER")]
    WatchlistReminder
}
