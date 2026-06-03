using StreetSignalApi.Common.Enums;
using StreetSignalApi.Common.Exceptions;
using StreetSignalApi.DTOs.Requests;
using StreetSignalApi.DTOs.Responses;
using StreetSignalApi.Mappers;
using StreetSignalApi.Models;
using StreetSignalApi.Repositories.Interfaces;
using StreetSignalApi.Services.Interfaces;

namespace StreetSignalApi.Services.Implementations;

public sealed class ReportUpdateService : IReportUpdateService
{
    private readonly IReportRepository _reports;
    private readonly IReportUpdateRepository _updates;
    private readonly INotificationRepository _notifications;

    public ReportUpdateService(
        IReportRepository reports,
        IReportUpdateRepository updates,
        INotificationRepository notifications)
    {
        _reports = reports;
        _updates = updates;
        _notifications = notifications;
    }

    public async Task<ReportUpdateListResponse> ListAsync(Guid reportId, Guid currentUserId, bool currentIsStaff, CancellationToken ct = default)
    {
        var report = await _reports.GetByIdAsync(reportId, includeUpdates: false, ct)
            ?? throw new NotFoundException("Report not found.");

        if (!currentIsStaff && report.CreatedById != currentUserId)
            throw new ForbiddenException();

        var items = await _updates.ListByReportAsync(reportId, ct);
        return new ReportUpdateListResponse
        {
            Data = items.Select(u => u.ToDto()).ToList()
        };
    }

    public async Task<ReportUpdateResponse> CreateAsync(Guid reportId, CreateReportUpdateRequest req, Guid currentUserId, bool currentIsStaff, CancellationToken ct = default)
    {
        var report = await _reports.GetByIdAsync(reportId, includeUpdates: false, ct)
            ?? throw new NotFoundException("Report not found.");

        if (!currentIsStaff && report.CreatedById != currentUserId)
            throw new ForbiddenException();

        var entity = new ReportUpdate
        {
            ReportId = reportId,
            UserId = currentUserId,
            Type = ReportUpdateType.Comment,
            Message = req.Message.Trim()
        };

        await _updates.AddAsync(entity, ct);

        // Staff comments notify the citizen
        if (currentIsStaff && report.CreatedById != currentUserId)
        {
            await _notifications.AddAsync(new Notification
            {
                UserId = report.CreatedById,
                ReportId = report.Id,
                Title = "New comment on your report",
                Message = req.Message.Trim()
            }, ct);
        }

        await _updates.SaveChangesAsync(ct);

        var saved = await _updates.GetByIdAsync(entity.Id, ct)!;
        return new ReportUpdateResponse { Data = saved!.ToDto() };
    }
}
