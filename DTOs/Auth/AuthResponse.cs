namespace ReviewFilms.Api.DTOs.Auth;

public sealed class AuthResponse
{
    public string Token { get; init; } = string.Empty;

    public string RefreshToken { get; init; } = string.Empty;

    public string TokenType { get; init; } = "Bearer";

    public DateTime ExpiresAt { get; init; }

    public Guid UserId { get; init; }

    public string Username { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string? AvatarUrl { get; init; }

    public string[] Roles { get; init; } = [];
}
