using NpgsqlTypes;

namespace ReviewFilms.Api.Enums;

public enum ReportTargetType
{
    [PgName("COMMENT")]
    Comment,

    [PgName("MOVIE")]
    Movie,

    [PgName("USER")]
    User
}
