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
    private readonly IPushNotificationService _push;
    private readonly ILogger<ReportUpdateService> _logger;

    public ReportUpdateService(
        IReportRepository reports,
        IReportUpdateRepository updates,
        INotificationRepository notifications,
        IPushNotificationService push,
        ILogger<ReportUpdateService> logger)
    {
        _reports = reports;
        _updates = updates;
        _notifications = notifications;
        _push = push;
        _logger = logger;
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
            CreatedById = currentUserId,
            Type = ReportUpdateType.Comment,
            Message = req.Message.Trim(),
            IsOfficial = currentIsStaff
        };

        await _updates.AddAsync(entity, ct);

        // Staff comments notify the citizen
        if (currentIsStaff && report.CreatedById != currentUserId)
        {
            const string notifTitle = "New comment on your report";
            var notifBody = req.Message.Trim();

            await _notifications.AddAsync(new Notification
            {
                UserId = report.CreatedById,
                ReportId = report.Id,
                Title = notifTitle,
                Message = notifBody
            }, ct);
        }

        await _updates.SaveChangesAsync(ct);

        var saved = await _updates.GetByIdAsync(entity.Id, ct)!;

        if (currentIsStaff && report.CreatedById != currentUserId)
        {
            await TrySendPushAsync(report.CreatedById, "New comment on your report", req.Message.Trim());
        }

        return new ReportUpdateResponse { Data = saved!.ToDto() };
    }

    private async Task TrySendPushAsync(Guid userId, string title, string body)
    {
        try
        {
            await _push.SendAsync(userId, title, body, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Push notification failed for user {UserId}", userId);
        }
    }
}
