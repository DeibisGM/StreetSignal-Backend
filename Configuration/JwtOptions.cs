namespace StreetSignalApi.Configuration;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "StreetSignal";
    public string Audience { get; set; } = "StreetSignal";
    public string SigningKey { get; set; } = string.Empty;
    public int ExpiresInSeconds { get; set; } = 86400; // 24h
}
