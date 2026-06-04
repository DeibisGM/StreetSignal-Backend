using StreetSignalApi.Models;

namespace StreetSignalApi.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> ListAsync(bool includeInactive, CancellationToken ct = default);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
