using NpgsqlTypes;

namespace ReviewFilms.Api.Enums;

public enum UserStatus
{
    [PgName("ACTIVE")]
    Active,

    [PgName("INACTIVE")]
    Inactive,

    [PgName("BANNED")]
    Banned,

    [PgName("PENDING")]
    Pending
}
