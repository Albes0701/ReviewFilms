using System.ComponentModel.DataAnnotations;

namespace ReviewFilms.Api.DTOs.Auth;

public sealed class LoginRequest
{
    [Required]
    [StringLength(255)]
    public string UsernameOrEmail { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Password { get; init; } = string.Empty;
}
