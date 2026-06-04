using StreetSignalApi.DTOs.Requests;
using StreetSignalApi.DTOs.Responses;

namespace StreetSignalApi.Services.Interfaces;

public interface IReportUpdateService
{
    Task<ReportUpdateListResponse> ListAsync(Guid reportId, Guid currentUserId, bool currentIsStaff, CancellationToken ct = default);
    Task<ReportUpdateResponse> CreateAsync(Guid reportId, CreateReportUpdateRequest req, Guid currentUserId, bool currentIsStaff, CancellationToken ct = default);
}
