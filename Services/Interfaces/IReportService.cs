using StreetSignalApi.Common.Enums;
using StreetSignalApi.DTOs.Requests;
using StreetSignalApi.DTOs.Responses;

namespace StreetSignalApi.Services.Interfaces;

public interface IReportService
{
    Task<PaginatedReportListResponse> ListForStaffAsync(
        ReportStatus? status, Guid? categoryId, string? search,
        DateTime? fromDate, DateTime? toDate, int page, int pageSize,
        CancellationToken ct = default);

    Task<PaginatedReportListResponse> ListMyAsync(
        Guid currentUserId, ReportStatus? status, Guid? categoryId, string? search,
        int page, int pageSize, CancellationToken ct = default);

    Task<ReportDetailResponse> GetByIdAsync(Guid id, Guid currentUserId, bool currentIsStaff, CancellationToken ct = default);

    Task<ReportDetailResponse> CreateAsync(CreateReportRequest req, Guid currentUserId, CancellationToken ct = default);

    Task<ReportDetailResponse> UpdateAsync(Guid id, UpdateReportRequest req, Guid currentUserId, CancellationToken ct = default);

    Task<ReportDetailResponse> ChangeStatusAsync(Guid id, ChangeReportStatusRequest req, Guid currentUserId, CancellationToken ct = default);
}
