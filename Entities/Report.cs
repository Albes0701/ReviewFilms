using ReviewFilms.Api.Enums;

namespace ReviewFilms.Api.Entities;

public class Report
{
    public Guid Id { get; set; }

    public Guid ReporterUserId { get; set; }

    public ReportTargetType TargetType { get; set; }

    public Guid TargetId { get; set; }

    public string ReasonCode { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ReportStatus Status { get; set; }

    public Guid? ReviewedByUserId { get; set; }

    public string? ResolutionNote { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public User ReporterUser { get; set; } = null!;

    public User? ReviewedByUser { get; set; }
}
