using NpgsqlTypes;

namespace ReviewFilms.Api.Enums;

public enum CommentStatus
{
    [PgName("VISIBLE")]
    Visible,

    [PgName("HIDDEN")]
    Hidden,

    [PgName("DELETED")]
    Deleted
}
