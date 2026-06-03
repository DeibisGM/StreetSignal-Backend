using StreetSignalApi.Common.Enums;

namespace StreetSignalApi.Models;

public class ReportUpdate
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ReportId { get; set; }
    public Report? Report { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public ReportUpdateType Type { get; set; } = ReportUpdateType.Comment;
    public string Message { get; set; } = string.Empty;

    public ReportStatus? OldStatus { get; set; }
    public ReportStatus? NewStatus { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
