namespace SkillHub.Infrastructure.Identity;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "SkillHub";
    public string Audience { get; set; } = "SkillHub";

    /// <summary>HMAC-SHA256 signing key; at least 32 characters.</summary>
    public string Key { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; } = 120;
}
