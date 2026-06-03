using StreetSignalApi.Common.Enums;

namespace StreetSignalApi.DTOs.Responses;

public class ReportUpdateDto
{
    public Guid Id { get; set; }
    public Guid ReportId { get; set; }
    public UserBasicDto User { get; set; } = new();
    public ReportUpdateType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public ReportStatus? OldStatus { get; set; }
    public ReportStatus? NewStatus { get; set; }
    public DateTime CreatedAt { get; set; }
}
