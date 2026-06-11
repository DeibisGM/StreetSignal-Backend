using StreetSignalApi.DTOs.Requests;
using StreetSignalApi.DTOs.Responses;
using StreetSignalApi.Models;

namespace StreetSignalApi.Mappers;

public static class ReportMapper
{
    public static Report ToEntity(this CreateReportRequest req, Guid createdById) => new()
    {
        Title = req.Title.Trim(),
        Description = req.Description.Trim(),
        CategoryId = req.CategoryId,
        ImageUrl = req.ImageUrl,
        Latitude = req.Latitude,
        Longitude = req.Longitude,
        Address = req.Address,
        CreatedById = createdById
    };

    public static ReportSummaryDto ToSummaryDto(this Report r) => new()
    {
        Id = r.Id,
        Title = r.Title,
        Description = r.Description,
        Status = r.Status,
        Priority = r.Priority,
        Category = r.Category?.ToDto() ?? new CategoryDto { Id = r.CategoryId },
        ImageUrl = r.ImageUrl,
        Latitude = r.Latitude ?? 0,
        Longitude = r.Longitude ?? 0,
        Address = r.Address,
        CreatedBy = r.CreatedBy?.ToBasicDto() ?? new UserBasicDto { Id = r.CreatedById },
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        ResolvedAt = r.ResolvedAt
    };

    public static ReportDetailDto ToDetailDto(this Report r)
    {
        var summary = r.ToSummaryDto();
        return new ReportDetailDto
        {
            Id = summary.Id,
            Title = summary.Title,
            Description = summary.Description,
            Status = summary.Status,
            Priority = summary.Priority,
            Category = summary.Category,
            ImageUrl = summary.ImageUrl,
            Latitude = summary.Latitude,
            Longitude = summary.Longitude,
            Address = summary.Address,
            CreatedBy = summary.CreatedBy,
            CreatedAt = summary.CreatedAt,
            UpdatedAt = summary.UpdatedAt,
            ResolvedAt = summary.ResolvedAt,
            Updates = r.Updates
                .OrderBy(u => u.CreatedAt)
                .Select(u => u.ToDto())
                .ToList()
        };
    }
}
