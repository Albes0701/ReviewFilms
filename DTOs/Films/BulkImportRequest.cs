using System.ComponentModel.DataAnnotations;

namespace ReviewFilms.Api.DTOs.Films;

public sealed class BulkImportRequest
{
    [Range(1, 200)]
    public int Count { get; init; } = 20;
}
