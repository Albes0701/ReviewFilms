using NpgsqlTypes;

namespace ReviewFilms.Api.Enums;

public enum WatchlistStatus
{
    [PgName("PLAN_TO_WATCH")]
    PlanToWatch,

    [PgName("WATCHING")]
    Watching,

    [PgName("WATCHED")]
    Watched,

    [PgName("DROPPED")]
    Dropped
}
