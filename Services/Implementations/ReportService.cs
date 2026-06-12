using StreetSignalApi.Common.Enums;
using StreetSignalApi.Common.Errors;
using StreetSignalApi.Common.Exceptions;
using StreetSignalApi.DTOs.Requests;
using StreetSignalApi.DTOs.Responses;
using StreetSignalApi.Mappers;
using StreetSignalApi.Models;
using StreetSignalApi.Repositories.Interfaces;
using StreetSignalApi.Services.Interfaces;

namespace StreetSignalApi.Services.Implementations;

public sealed class ReportService : IReportService
{
    private readonly IReportRepository _reports;
    private readonly ICategoryRepository _categories;
    private readonly IReportUpdateRepository _updates;
    private readonly INotificationRepository _notifications;
    private readonly IUserRepository _users;
    private readonly IPushNotificationService _push;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        IReportRepository reports,
        ICategoryRepository categories,
        IReportUpdateRepository updates,
        INotificationRepository notifications,
        IUserRepository users,
        IPushNotificationService push,
        ILogger<ReportService> logger)
    {
        _reports = reports;
        _categories = categories;
        _updates = updates;
        _notifications = notifications;
        _users = users;
        _push = push;
        _logger = logger;
    }

    public async Task<PaginatedReportListResponse> ListForStaffAsync(
        ReportStatus? status, Guid? categoryId, string? search,
        DateTime? fromDate, DateTime? toDate, int page, int pageSize,
        CancellationToken ct = default)
    {
        var (p, ps) = NormalizePaging(page, pageSize);
        var result = await _reports.ListAsync(
            new ReportFilter(status, categoryId, search, fromDate, toDate, null, p, ps), ct);
        return BuildList(result, p, ps);
    }

    public async Task<PaginatedReportListResponse> ListMyAsync(
        Guid currentUserId, ReportStatus? status, Guid? categoryId, string? search,
        int page, int pageSize, CancellationToken ct = default)
    {
        var (p, ps) = NormalizePaging(page, pageSize);
        var result = await _reports.ListAsync(
            new ReportFilter(status, categoryId, search, null, null, currentUserId, p, ps), ct);
        return BuildList(result, p, ps);
    }

    public async Task<ReportDetailResponse> GetByIdAsync(Guid id, Guid currentUserId, bool currentIsStaff, CancellationToken ct = default)
    {
        var report = await _reports.GetByIdAsync(id, includeUpdates: true, ct)
            ?? throw new NotFoundException("Report not found.");

        if (!currentIsStaff && report.CreatedById != currentUserId)
            throw new ForbiddenException();

        return new ReportDetailResponse { Data = report.ToDetailDto() };
    }

    public async Task<ReportDetailResponse> CreateAsync(CreateReportRequest req, Guid currentUserId, CancellationToken ct = default)
    {
        var category = await _categories.GetByIdAsync(req.CategoryId, ct);
        if (category is null || !category.IsActive)
        {
            throw new BadRequestException(ErrorCodes.ValidationError, "Category is not valid or not active.");
        }

        var report = req.ToEntity(currentUserId);
        report.Status = ReportStatus.Pending;

        await _reports.AddAsync(report, ct);

        // Initial "System" entry to bootstrap the timeline
        await _updates.AddAsync(new ReportUpdate
        {
            ReportId = report.Id,
            CreatedById = currentUserId,
            Type = ReportUpdateType.System,
            Message = "Report submitted by the citizen.",
            NewStatus = ReportStatus.Pending,
            IsOfficial = false
        }, ct);

        var staffUsers = await _users.ListStaffAsync(ct);
        const string staffNotifTitle = "Nuevo reporte";
        var staffNotifBody = $"Se registró un nuevo reporte: {report.Title}.";

        foreach (var staff in staffUsers)
        {
            await _notifications.AddAsync(new Notification
            {
                UserId = staff.Id,
                ReportId = report.Id,
                Title = staffNotifTitle,
                Message = staffNotifBody
            }, ct);
        }

        await _reports.SaveChangesAsync(ct);

        var saved = await _reports.GetByIdAsync(report.Id, includeUpdates: true, ct)!;

        foreach (var staff in staffUsers)
        {
            await TrySendPushAsync(staff.Id, staffNotifTitle, staffNotifBody);
        }

        return new ReportDetailResponse { Data = saved!.ToDetailDto() };
    }

    public async Task<ReportDetailResponse> UpdateAsync(Guid id, UpdateReportRequest req, Guid currentUserId, CancellationToken ct = default)
    {
        var report = await _reports.GetByIdAsync(id, includeUpdates: true, ct)
            ?? throw new NotFoundException("Report not found.");

        if (report.CreatedById != currentUserId)
            throw new ForbiddenException("Only the report owner can edit it.");

        if (report.Status != ReportStatus.Pending)
            throw new ConflictException(ErrorCodes.ReportNotEditable,
                "Only pending reports can be edited by citizens.");

        if (!string.IsNullOrWhiteSpace(req.Title)) report.Title = req.Title.Trim();
        if (!string.IsNullOrWhiteSpace(req.Description)) report.Description = req.Description.Trim();
        if (req.CategoryId.HasValue)
        {
            var cat = await _categories.GetByIdAsync(req.CategoryId.Value, ct);
            if (cat is null || !cat.IsActive)
                throw new BadRequestException(ErrorCodes.ValidationError, "Category is not valid or not active.");
            report.CategoryId = req.CategoryId.Value;
        }
        if (req.ImageUrl is not null) report.ImageUrl = req.ImageUrl;

        report.UpdatedAt = DateTime.UtcNow;
        await _reports.SaveChangesAsync(ct);

        var saved = await _reports.GetByIdAsync(report.Id, includeUpdates: true, ct)!;
        return new ReportDetailResponse { Data = saved!.ToDetailDto() };
    }

    public async Task<ReportDetailResponse> ChangeStatusAsync(Guid id, ChangeReportStatusRequest req, Guid currentUserId, CancellationToken ct = default)
    {
        var report = await _reports.GetByIdAsync(id, includeUpdates: true, ct)
            ?? throw new NotFoundException("Report not found.");

        var oldStatus = report.Status;
        var newStatus = req.Status;
        var statusChanged = oldStatus != newStatus;
        var oldPriority = report.Priority;
        var oldAssignedToId = report.AssignedToId;

        User? assignedTo = null;
        if (req.AssignedToId.HasValue)
        {
            assignedTo = await _users.GetByIdAsync(req.AssignedToId.Value, ct);
            if (assignedTo is null || assignedTo.Role != UserRole.Staff || !assignedTo.IsActive)
            {
                throw new BadRequestException(ErrorCodes.ValidationError, "Assigned staff member is not valid.");
            }
        }

        report.Status = newStatus;
        if (req.Priority.HasValue)
        {
            report.Priority = req.Priority.Value;
        }
        if (req.AssignedToId.HasValue)
        {
            report.AssignedToId = req.AssignedToId.Value;
            report.AssignedTo = assignedTo;
        }
        report.UpdatedAt = DateTime.UtcNow;
        if (statusChanged && newStatus == ReportStatus.Resolved) report.ResolvedAt = DateTime.UtcNow;
        if (statusChanged && newStatus != ReportStatus.Resolved && oldStatus == ReportStatus.Resolved)
        {
            report.ResolvedAt = null;
        }

        var changed = oldStatus != newStatus
            || oldPriority != report.Priority
            || oldAssignedToId != report.AssignedToId;

        if (!changed && string.IsNullOrWhiteSpace(req.Message))
        {
            throw new BadRequestException(ErrorCodes.ValidationError, "No changes were provided.");
        }

        var changeSummary = BuildChangeSummary(
            oldStatus,
            newStatus,
            oldPriority,
            report.Priority,
            oldAssignedToId != report.AssignedToId,
            report.AssignedTo?.FullName);

        await _updates.AddAsync(new ReportUpdate
        {
            ReportId = report.Id,
            CreatedById = currentUserId,
            Type = statusChanged ? ReportUpdateType.StatusChange : ReportUpdateType.System,
            Message = string.IsNullOrWhiteSpace(req.Message)
                ? changeSummary
                : req.Message.Trim(),
            OldStatus = statusChanged ? oldStatus : null,
            NewStatus = statusChanged ? newStatus : null,
            IsOfficial = true
        }, ct);

        const string notifTitle = "Report status updated";
        var notifBody = $"Your report is now {newStatus}.";

        await _notifications.AddAsync(new Notification
        {
            UserId = report.CreatedById,
            ReportId = report.Id,
            Title = notifTitle,
            Message = notifBody
        }, ct);

        await _reports.SaveChangesAsync(ct);

        var saved = await _reports.GetByIdAsync(report.Id, includeUpdates: true, ct)!;

        await TrySendPushAsync(report.CreatedById, notifTitle, notifBody);

        return new ReportDetailResponse { Data = saved!.ToDetailDto() };
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

    private static string BuildChangeSummary(
        ReportStatus oldStatus,
        ReportStatus newStatus,
        Priority? oldPriority,
        Priority? newPriority,
        bool assignedChanged,
        string? assignedToName)
    {
        var parts = new List<string>();

        if (oldStatus != newStatus)
        {
            parts.Add($"Status changed from {oldStatus} to {newStatus}");
        }

        if (oldPriority != newPriority && newPriority.HasValue)
        {
            parts.Add($"Priority set to {newPriority}");
        }

        if (assignedChanged && !string.IsNullOrWhiteSpace(assignedToName))
        {
            parts.Add($"Assigned to {assignedToName}");
        }

        return parts.Count > 0
            ? string.Join(". ", parts) + "."
            : $"Status updated to {newStatus}.";
    }

    private static (int Page, int PageSize) NormalizePaging(int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;
        return (page, pageSize);
    }

    private static PaginatedReportListResponse BuildList(PagedResult<Report> result, int page, int pageSize) => new()
    {
        Data = result.Items.Select(r => r.ToSummaryDto()).ToList(),
        Pagination = PaginationMeta.For(page, pageSize, result.TotalItems)
    };
}
