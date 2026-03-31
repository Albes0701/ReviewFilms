using System.ComponentModel.DataAnnotations;

namespace ReviewFilms.Api.Configurations;

public sealed class CloudinarySettings
{
    public const string SectionName = "Cloudinary";

    [Required]
    public string CloudName { get; set; } = string.Empty;

    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    public string ApiSecret { get; set; } = string.Empty;

    public string? DefaultFolder { get; set; }

    public string? UploadPreset { get; set; }
}
