namespace StreetSignalApi.DTOs.Responses;

public class HealthResponse
{
    public string Status { get; set; } = "ok";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
