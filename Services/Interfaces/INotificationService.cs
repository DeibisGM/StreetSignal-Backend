using StreetSignalApi.DTOs.Requests;
using StreetSignalApi.DTOs.Responses;

namespace StreetSignalApi.Services.Interfaces;

public interface INotificationService
{
    Task<PaginatedNotificationListResponse> ListAsync(Guid userId, bool unreadOnly, int page, int pageSize, CancellationToken ct = default);
    Task MarkAsReadAsync(Guid notificationId, Guid currentUserId, CancellationToken ct = default);
    Task RegisterDeviceTokenAsync(RegisterDeviceTokenRequest req, Guid currentUserId, CancellationToken ct = default);
}
