using StreetSignalApi.Common.Enums;

namespace StreetSignalApi.Models;

public class DeviceToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string Token { get; set; } = string.Empty;
    public Platform Platform { get; set; } = Platform.Unknown;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
