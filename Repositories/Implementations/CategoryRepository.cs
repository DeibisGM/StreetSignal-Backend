using Microsoft.EntityFrameworkCore;
using StreetSignalApi.Data;
using StreetSignalApi.Models;
using StreetSignalApi.Repositories.Interfaces;

namespace StreetSignalApi.Repositories.Implementations;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;
    public CategoryRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Category>> ListAsync(bool includeInactive, CancellationToken ct = default)
    {
        var q = _db.Categories.AsQueryable();
        if (!includeInactive) q = q.Where(c => c.IsActive);
        return await q.OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync(ct);
    }

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);
}
