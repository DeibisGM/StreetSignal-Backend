using StreetSignalApi.Common.Enums;

namespace StreetSignalApi.Models;

public class Report
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public string? ImageUrl { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }

    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public Priority? Priority { get; set; }

    public Guid CreatedById { get; set; }
    public User? CreatedBy { get; set; }

    public Guid? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    public ICollection<ReportUpdate> Updates { get; set; } = new List<ReportUpdate>();
}
