using System.ComponentModel.DataAnnotations;

namespace ReviewFilms.Api.DTOs.Auth;

public sealed class RefreshRequest
{
    [Required]
    [StringLength(4000)]
    public string AccessToken { get; init; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string RefreshToken { get; init; } = string.Empty;
}
