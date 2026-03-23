using NpgsqlTypes;

namespace ReviewFilms.Api.Enums;

public enum CreditType
{
    [PgName("CAST")]
    Cast,

    [PgName("CREW")]
    Crew
}
