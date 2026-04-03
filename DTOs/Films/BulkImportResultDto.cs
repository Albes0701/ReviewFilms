namespace ReviewFilms.Api.DTOs.Films;

public sealed class BulkImportResultDto
{
    public int RequestedCount { get; init; }

    public int ReviewedCount { get; init; }

    public int ImportedCount { get; init; }

    public int SkippedCount { get; init; }

    public int FailedCount { get; init; }

    public string Message { get; init; } = string.Empty;
}
