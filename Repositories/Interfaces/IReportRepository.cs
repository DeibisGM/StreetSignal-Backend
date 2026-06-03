using StreetSignalApi.Common.Enums;
using StreetSignalApi.Models;

namespace StreetSignalApi.Repositories.Interfaces;

public record ReportFilter(
    ReportStatus? Status,
    Guid? CategoryId,
    string? Search,
    DateTime? FromDate,
    DateTime? ToDate,
    Guid? CreatedById,
    int Page,
    int PageSize
);

public record PagedResult<T>(IReadOnlyList<T> Items, int TotalItems);

public interface IReportRepository
{
    Task<PagedResult<Report>> ListAsync(ReportFilter filter, CancellationToken ct = default);
    Task<Report?> GetByIdAsync(Guid id, bool includeUpdates = false, CancellationToken ct = default);
    Task AddAsync(Report report, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
