using StreetSignalApi.Common.Enums;
using StreetSignalApi.Models;

namespace StreetSignalApi.Repositories.Interfaces;

public interface IDeviceTokenRepository
{
    Task<DeviceToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task<IReadOnlyList<DeviceToken>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task UpsertAsync(Guid userId, string token, Platform? platform, CancellationToken ct = default);
    Task RemoveAsync(string token, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
