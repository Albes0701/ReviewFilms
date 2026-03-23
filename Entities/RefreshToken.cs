namespace ReviewFilms.Api.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public string? JwtId { get; set; }

    public string? DeviceName { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public Guid? ReplacedByTokenId { get; set; }

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;

    public RefreshToken? ReplacedByToken { get; set; }

    public ICollection<RefreshToken> ReplacedTokens { get; set; } = new List<RefreshToken>();
}
