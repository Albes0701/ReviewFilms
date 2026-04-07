using System.ComponentModel.DataAnnotations;

namespace ReviewFilms.Api.DTOs.Auth;

public sealed class LogoutRequest
{
    [StringLength(500)]
    public string? RefreshToken { get; init; }
}
