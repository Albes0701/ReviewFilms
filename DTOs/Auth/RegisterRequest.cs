using System.ComponentModel.DataAnnotations;

namespace ReviewFilms.Api.DTOs.Auth;

public sealed class RegisterRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; init; } = string.Empty;

    [StringLength(100)]
    public string? DisplayName { get; init; }
}
