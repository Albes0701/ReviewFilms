using NpgsqlTypes;

namespace ReviewFilms.Api.Enums;

public enum ReportStatus
{
    [PgName("PENDING")]
    Pending,

    [PgName("IN_REVIEW")]
    InReview,

    [PgName("RESOLVED")]
    Resolved,

    [PgName("REJECTED")]
    Rejected
}
