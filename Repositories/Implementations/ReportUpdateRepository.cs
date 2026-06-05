using Microsoft.EntityFrameworkCore;
using StreetSignalApi.Data;
using StreetSignalApi.Models;
using StreetSignalApi.Repositories.Interfaces;

namespace StreetSignalApi.Repositories.Implementations;

public sealed class ReportUpdateRepository : IReportUpdateRepository
{
    private readonly AppDbContext _db;
    public ReportUpdateRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<ReportUpdate>> ListByReportAsync(Guid reportId, CancellationToken ct = default) =>
        await _db.ReportUpdates
            .Include(u => u.CreatedBy)
            .Where(u => u.ReportId == reportId)
            .OrderBy(u => u.CreatedAt)
            .ToListAsync(ct);

    public Task<ReportUpdate?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.ReportUpdates.Include(u => u.CreatedBy).FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task AddAsync(ReportUpdate update, CancellationToken ct = default) =>
        await _db.ReportUpdates.AddAsync(update, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
