namespace ReviewFilms.Api.Configurations;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;

    public string Issuer { get; set; } = "ReviewFilms.Api";

    public string Audience { get; set; } = "ReviewFilms.Frontend";

    public int AccessTokenMinutes { get; set; } = 60;

    public int RefreshTokenDays { get; set; } = 30;
}
