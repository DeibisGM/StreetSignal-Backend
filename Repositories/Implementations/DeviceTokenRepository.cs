using Microsoft.EntityFrameworkCore;
using StreetSignalApi.Common.Enums;
using StreetSignalApi.Data;
using StreetSignalApi.Models;
using StreetSignalApi.Repositories.Interfaces;

namespace StreetSignalApi.Repositories.Implementations;

public sealed class DeviceTokenRepository : IDeviceTokenRepository
{
    private readonly AppDbContext _db;
    public DeviceTokenRepository(AppDbContext db) => _db = db;

    public Task<DeviceToken?> GetByTokenAsync(string token, CancellationToken ct = default) =>
        _db.DeviceTokens.FirstOrDefaultAsync(d => d.Token == token, ct);

    public async Task UpsertAsync(Guid userId, string token, Platform? platform, CancellationToken ct = default)
    {
        var existing = await _db.DeviceTokens.FirstOrDefaultAsync(d => d.Token == token, ct);
        if (existing is null)
        {
            await _db.DeviceTokens.AddAsync(new DeviceToken
            {
                UserId = userId,
                Token = token,
                Platform = platform ?? Platform.Unknown,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }, ct);
        }
        else
        {
            existing.UserId = userId;
            existing.Platform = platform ?? existing.Platform;
            existing.UpdatedAt = DateTime.UtcNow;
        }
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
