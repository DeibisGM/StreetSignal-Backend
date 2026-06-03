namespace StreetSignalApi.DTOs.Responses;

public class PaginatedNotificationListResponse
{
    public IReadOnlyList<NotificationDto> Data { get; set; } = Array.Empty<NotificationDto>();
    public PaginationMeta Pagination { get; set; } = new();
}
