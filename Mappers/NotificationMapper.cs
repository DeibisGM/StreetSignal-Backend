using StreetSignalApi.DTOs.Responses;
using StreetSignalApi.Models;

namespace StreetSignalApi.Mappers;

public static class NotificationMapper
{
    public static NotificationDto ToDto(this Notification n) => new()
    {
        Id = n.Id,
        UserId = n.UserId,
        ReportId = n.ReportId,
        Title = n.Title,
        Message = n.Message,
        IsRead = n.IsRead,
        CreatedAt = n.CreatedAt
    };
}
