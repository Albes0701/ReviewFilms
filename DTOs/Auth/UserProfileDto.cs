using ReviewFilms.Api.Enums;

namespace ReviewFilms.Api.DTOs.Auth;

public sealed class UserProfileDto
{
    public Guid UserId { get; init; }

    public string Username { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string? AvatarUrl { get; init; }

    public string? Bio { get; init; }

    public UserStatus Status { get; init; }

    public bool EmailConfirmed { get; init; }

    public DateTime? LastLoginAt { get; init; }

    public DateTime CreatedAt { get; init; }

    public string[] Roles { get; init; } = [];

    public string[] Permissions { get; init; } = [];
}
