using Microsoft.EntityFrameworkCore;
using StreetSignalApi.Data;
using StreetSignalApi.Models;
using StreetSignalApi.Repositories.Interfaces;

namespace StreetSignalApi.Repositories.Implementations;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _db;
    public NotificationRepository(AppDbContext db) => _db = db;

    public async Task<PagedResult<Notification>> ListForUserAsync(Guid userId, bool unreadOnly, int page, int pageSize, CancellationToken ct = default)
    {
        var q = _db.Notifications.Where(n => n.UserId == userId);
        if (unreadOnly) q = q.Where(n => !n.IsRead);

        var total = await q.CountAsync(ct);

        var items = await q.OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Notification>(items, total);
    }

    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Notifications.FirstOrDefaultAsync(n => n.Id == id, ct);

    public async Task AddAsync(Notification notification, CancellationToken ct = default) =>
        await _db.Notifications.AddAsync(notification, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
