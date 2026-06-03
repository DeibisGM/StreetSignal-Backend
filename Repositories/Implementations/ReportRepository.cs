using Microsoft.EntityFrameworkCore;
using StreetSignalApi.Data;
using StreetSignalApi.Models;
using StreetSignalApi.Repositories.Interfaces;

namespace StreetSignalApi.Repositories.Implementations;

public sealed class ReportRepository : IReportRepository
{
    private readonly AppDbContext _db;
    public ReportRepository(AppDbContext db) => _db = db;

    public async Task<PagedResult<Report>> ListAsync(ReportFilter filter, CancellationToken ct = default)
    {
        var q = _db.Reports
            .Include(r => r.Category)
            .Include(r => r.CreatedBy)
            .Include(r => r.AssignedTo)
            .AsQueryable();

        if (filter.Status.HasValue)         q = q.Where(r => r.Status == filter.Status.Value);
        if (filter.CategoryId.HasValue)     q = q.Where(r => r.CategoryId == filter.CategoryId.Value);
        if (filter.CreatedById.HasValue)    q = q.Where(r => r.CreatedById == filter.CreatedById.Value);
        if (filter.FromDate.HasValue)       q = q.Where(r => r.CreatedAt >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)         q = q.Where(r => r.CreatedAt <= filter.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            q = q.Where(r =>
                r.Title.ToLower().Contains(s) ||
                r.Description.ToLower().Contains(s) ||
                (r.Address != null && r.Address.ToLower().Contains(s)));
        }

        var total = await q.CountAsync(ct);

        var items = await q.OrderByDescending(r => r.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Report>(items, total);
    }

    public Task<Report?> GetByIdAsync(Guid id, bool includeUpdates = false, CancellationToken ct = default)
    {
        var q = _db.Reports
            .Include(r => r.Category)
            .Include(r => r.CreatedBy)
            .Include(r => r.AssignedTo)
            .AsQueryable();

        if (includeUpdates)
            q = q.Include(r => r.Updates).ThenInclude(u => u.User);

        return q.FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task AddAsync(Report report, CancellationToken ct = default) =>
        await _db.Reports.AddAsync(report, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
