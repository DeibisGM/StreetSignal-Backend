using StreetSignalApi.Common.Enums;

namespace StreetSignalApi.DTOs.Responses;

public class ReportSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ReportStatus Status { get; set; }
    public Priority? Priority { get; set; }
    public CategoryDto Category { get; set; } = new();
    public string? ImageUrl { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Address { get; set; }
    public UserBasicDto CreatedBy { get; set; } = new();
    public UserBasicDto? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
