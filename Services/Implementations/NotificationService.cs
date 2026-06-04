using StreetSignalApi.Common.Exceptions;
using StreetSignalApi.DTOs.Requests;
using StreetSignalApi.DTOs.Responses;
using StreetSignalApi.Mappers;
using StreetSignalApi.Repositories.Interfaces;
using StreetSignalApi.Services.Interfaces;

namespace StreetSignalApi.Services.Implementations;

public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _notifications;
    private readonly IDeviceTokenRepository _deviceTokens;

    public NotificationService(INotificationRepository notifications, IDeviceTokenRepository deviceTokens)
    {
        _notifications = notifications;
        _deviceTokens = deviceTokens;
    }

    public async Task<PaginatedNotificationListResponse> ListAsync(Guid userId, bool unreadOnly, int page, int pageSize, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var result = await _notifications.ListForUserAsync(userId, unreadOnly, page, pageSize, ct);
        return new PaginatedNotificationListResponse
        {
            Data = result.Items.Select(n => n.ToDto()).ToList(),
            Pagination = PaginationMeta.For(page, pageSize, result.TotalItems)
        };
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid currentUserId, CancellationToken ct = default)
    {
        var notif = await _notifications.GetByIdAsync(notificationId, ct)
            ?? throw new NotFoundException("Notification not found.");

        if (notif.UserId != currentUserId) throw new ForbiddenException();

        if (!notif.IsRead)
        {
            notif.IsRead = true;
            await _notifications.SaveChangesAsync(ct);
        }
    }

    public async Task RegisterDeviceTokenAsync(RegisterDeviceTokenRequest req, Guid currentUserId, CancellationToken ct = default)
    {
        await _deviceTokens.UpsertAsync(currentUserId, req.Token.Trim(), req.Platform, ct);
        await _deviceTokens.SaveChangesAsync(ct);
    }
}
