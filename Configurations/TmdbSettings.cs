using System.ComponentModel.DataAnnotations;

namespace ReviewFilms.Api.Configurations;

public sealed class TmdbSettings
{
    public const string SectionName = "Tmdb";

    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    public string BaseUrl { get; set; } = string.Empty;

    [Required]
    public string ImageBaseUrl { get; set; } = string.Empty;
}
