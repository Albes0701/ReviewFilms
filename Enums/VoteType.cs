using NpgsqlTypes;

namespace ReviewFilms.Api.Enums;

public enum VoteType
{
    [PgName("UP")]
    Up,

    [PgName("DOWN")]
    Down
}
