using StreetSignalApi.DTOs.Responses;
using StreetSignalApi.Models;

namespace StreetSignalApi.Mappers;

public static class ReportUpdateMapper
{
    public static ReportUpdateDto ToDto(this ReportUpdate u) => new()
    {
        Id = u.Id,
        ReportId = u.ReportId,
        User = u.CreatedBy?.ToBasicDto() ?? new UserBasicDto { Id = u.CreatedById },
        Type = u.Type,
        Message = u.Message,
        IsOfficial = u.IsOfficial,
        OldStatus = u.OldStatus,
        NewStatus = u.NewStatus,
        CreatedAt = u.CreatedAt
    };
}
