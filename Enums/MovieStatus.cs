using NpgsqlTypes;

namespace ReviewFilms.Api.Enums;

public enum MovieStatus
{
    [PgName("DRAFT")]
    Draft,

    [PgName("PUBLISHED")]
    Published,

    [PgName("ARCHIVED")]
    Archived
}
