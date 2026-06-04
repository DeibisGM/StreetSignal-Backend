using StreetSignalApi.Models;
using StreetSignalApi.Repositories.Interfaces;

namespace StreetSignalApi.Repositories.Interfaces;

public interface INotificationRepository
{
    Task<PagedResult<Notification>> ListForUserAsync(Guid userId, bool unreadOnly, int page, int pageSize, CancellationToken ct = default);
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Notification notification, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
