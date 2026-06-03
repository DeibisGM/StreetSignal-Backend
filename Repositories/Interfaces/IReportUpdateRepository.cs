using StreetSignalApi.Models;

namespace StreetSignalApi.Repositories.Interfaces;

public interface IReportUpdateRepository
{
    Task<IReadOnlyList<ReportUpdate>> ListByReportAsync(Guid reportId, CancellationToken ct = default);
    Task<ReportUpdate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ReportUpdate update, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
