using StreetSignalApi.DTOs.Responses;
using StreetSignalApi.Models;

namespace StreetSignalApi.Mappers;

public static class ReportUpdateMapper
{
    public static ReportUpdateDto ToDto(this ReportUpdate u) => new()
    {
        Id = u.Id,
        ReportId = u.ReportId,
        User = u.User!.ToBasicDto(),
        Type = u.Type,
        Message = u.Message,
        OldStatus = u.OldStatus,
        NewStatus = u.NewStatus,
        CreatedAt = u.CreatedAt
    };
}
