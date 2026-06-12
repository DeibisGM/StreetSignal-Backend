using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using StreetSignalApi.Common.Enums;
using StreetSignalApi.Common.Errors;
using StreetSignalApi.Common.Exceptions;
using StreetSignalApi.DTOs.Requests;
using StreetSignalApi.Models;
using StreetSignalApi.Repositories.Interfaces;
using StreetSignalApi.Services.Implementations;
using StreetSignalApi.Services.Interfaces;

namespace StreetSignalApi.UnitTests.Services;

public class ReportServiceTests
{
    private readonly Mock<IReportRepository> _reports = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<IReportUpdateRepository> _updates = new();
    private readonly Mock<INotificationRepository> _notifications = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPushNotificationService> _push = new();
    private readonly Mock<ILogger<ReportService>> _logger = new();

    private ReportService Build() => new(_reports.Object, _categories.Object, _updates.Object, _notifications.Object, _users.Object, _push.Object, _logger.Object);

    private static Report MakeReport(Guid ownerId, ReportStatus status = ReportStatus.Pending) => new()
    {
        Id = Guid.NewGuid(),
        Title = "Title",
        Description = "Description",
        Status = status,
        CategoryId = Guid.NewGuid(),
        Category = new Category { Name = "Cat", IsActive = true },
        CreatedById = ownerId,
        CreatedBy = new User { Id = ownerId, FullName = "Owner", Role = UserRole.Citizen, Email = "o@x.com" },
        Latitude = 10,
        Longitude = -84,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task GetById_forbids_citizen_accessing_other_report()
    {
        var owner = Guid.NewGuid();
        var report = MakeReport(owner);
        _reports.Setup(r => r.GetByIdAsync(report.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(report);
        var sut = Build();

        var act = () => sut.GetByIdAsync(report.Id, Guid.NewGuid(), currentIsStaff: false);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task GetById_allows_staff_to_access_any_report()
    {
        var report = MakeReport(Guid.NewGuid());
        _reports.Setup(r => r.GetByIdAsync(report.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(report);
        var sut = Build();

        var result = await sut.GetByIdAsync(report.Id, Guid.NewGuid(), currentIsStaff: true);

        result.Data.Id.Should().Be(report.Id);
    }

    [Fact]
    public async Task GetById_throws_NotFound_when_missing()
    {
        _reports.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>())).ReturnsAsync((Report?)null);
        var sut = Build();

        var act = () => sut.GetByIdAsync(Guid.NewGuid(), Guid.NewGuid(), true);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Create_rejects_inactive_category()
    {
        var catId = Guid.NewGuid();
        _categories.Setup(c => c.GetByIdAsync(catId, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new Category { Id = catId, IsActive = false, Name = "X" });
        var sut = Build();

        var act = () => sut.CreateAsync(new CreateReportRequest
        {
            Title = "Title",
            Description = "Description longer",
            CategoryId = catId,
            Latitude = 10,
            Longitude = -84
        }, Guid.NewGuid());

        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.Which.Code.Should().Be(ErrorCodes.ValidationError);
    }

    [Fact]
    public async Task Create_notifies_each_active_staff_member()
    {
        var owner = Guid.NewGuid();
        var catId = Guid.NewGuid();
        Report? createdReport = null;

        _categories.Setup(c => c.GetByIdAsync(catId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category { Id = catId, IsActive = true, Name = "Cat" });
        _reports.Setup(r => r.AddAsync(It.IsAny<Report>(), It.IsAny<CancellationToken>()))
            .Callback<Report, CancellationToken>((report, _) => createdReport = report)
            .Returns(Task.CompletedTask);
        _reports.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
            .Returns((Guid id, bool includeUpdates, CancellationToken ct) => Task.FromResult(
                createdReport is null
                    ? null
                    : new Report
                    {
                        Id = createdReport.Id,
                        Title = createdReport.Title,
                        Description = createdReport.Description,
                        CategoryId = createdReport.CategoryId,
                        Category = new Category { Id = catId, IsActive = true, Name = "Cat" },
                        CreatedById = owner,
                        CreatedBy = new User { Id = owner, FullName = "Citizen", Role = UserRole.Citizen, Email = "c@x.com" },
                        Latitude = createdReport.Latitude,
                        Longitude = createdReport.Longitude,
                        Status = createdReport.Status,
                        CreatedAt = createdReport.CreatedAt,
                        UpdatedAt = createdReport.UpdatedAt
                    }));
        _users.Setup(u => u.ListStaffAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>
            {
                new() { Id = Guid.NewGuid(), FullName = "Staff One", Role = UserRole.Staff, Email = "s1@x.com" },
                new() { Id = Guid.NewGuid(), FullName = "Staff Two", Role = UserRole.Staff, Email = "s2@x.com" }
            });

        var sut = Build();

        await sut.CreateAsync(new CreateReportRequest
        {
            Title = "Broken streetlight",
            Description = "The lamp on the corner is out.",
            CategoryId = catId,
            Latitude = 10,
            Longitude = -84
        }, owner);

        createdReport.Should().NotBeNull();
        _notifications.Verify(n => n.AddAsync(It.Is<Notification>(x =>
            x.ReportId == createdReport!.Id &&
            x.Title == "Nuevo reporte" &&
            x.Message.Contains("Broken streetlight")), It.IsAny<CancellationToken>()), Times.Exactly(2));

        _push.Verify(p => p.SendAsync(It.IsAny<Guid>(), "Nuevo reporte", It.Is<string>(body => body.Contains("Broken streetlight")), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Create_returns_success_when_push_fails()
    {
        var owner = Guid.NewGuid();
        var catId = Guid.NewGuid();
        var staffId = Guid.NewGuid();
        Report? createdReport = null;

        _categories.Setup(c => c.GetByIdAsync(catId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category { Id = catId, IsActive = true, Name = "Cat" });
        _reports.Setup(r => r.AddAsync(It.IsAny<Report>(), It.IsAny<CancellationToken>()))
            .Callback<Report, CancellationToken>((report, _) => createdReport = report)
            .Returns(Task.CompletedTask);
        _reports.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
            .Returns((Guid id, bool includeUpdates, CancellationToken ct) => Task.FromResult(
                createdReport is null
                    ? null
                    : new Report
                    {
                        Id = createdReport.Id,
                        Title = createdReport.Title,
                        Description = createdReport.Description,
                        CategoryId = createdReport.CategoryId,
                        Category = new Category { Id = catId, IsActive = true, Name = "Cat" },
                        CreatedById = owner,
                        CreatedBy = new User { Id = owner, FullName = "Citizen", Role = UserRole.Citizen, Email = "c@x.com" },
                        Latitude = createdReport.Latitude,
                        Longitude = createdReport.Longitude,
                        Status = createdReport.Status,
                        CreatedAt = createdReport.CreatedAt,
                        UpdatedAt = createdReport.UpdatedAt
                    }));
        _users.Setup(u => u.ListStaffAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>
            {
                new() { Id = staffId, FullName = "Staff One", Role = UserRole.Staff, Email = "s1@x.com" }
            });
        _push.Setup(p => p.SendAsync(staffId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Firebase unavailable"));

        var sut = Build();

        var result = await sut.CreateAsync(new CreateReportRequest
        {
            Title = "Broken streetlight",
            Description = "The lamp on the corner is out.",
            CategoryId = catId,
            Latitude = 10,
            Longitude = -84
        }, owner);

        result.Data.Title.Should().Be("Broken streetlight");
    }

    [Fact]
    public async Task Update_forbids_non_owner()
    {
        var owner = Guid.NewGuid();
        var report = MakeReport(owner);
        _reports.Setup(r => r.GetByIdAsync(report.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(report);
        var sut = Build();

        var act = () => sut.UpdateAsync(report.Id, new UpdateReportRequest { Title = "New title" }, Guid.NewGuid());

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Update_throws_conflict_when_report_not_pending()
    {
        var owner = Guid.NewGuid();
        var report = MakeReport(owner, ReportStatus.InProgress);
        _reports.Setup(r => r.GetByIdAsync(report.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(report);
        var sut = Build();

        var act = () => sut.UpdateAsync(report.Id, new UpdateReportRequest { Title = "New title" }, owner);

        var ex = await act.Should().ThrowAsync<ConflictException>();
        ex.Which.Code.Should().Be(ErrorCodes.ReportNotEditable);
    }

    [Fact]
    public async Task ChangeStatus_adds_StatusChange_update_and_notification()
    {
        var owner = Guid.NewGuid();
        var report = MakeReport(owner, ReportStatus.Pending);
        _reports.Setup(r => r.GetByIdAsync(report.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(report);
        var sut = Build();

        await sut.ChangeStatusAsync(report.Id,
            new ChangeReportStatusRequest { Status = ReportStatus.InProgress, Message = "Assigned" },
            currentUserId: Guid.NewGuid());

        _updates.Verify(u => u.AddAsync(It.Is<ReportUpdate>(x =>
            x.Type == ReportUpdateType.StatusChange &&
            x.OldStatus == ReportStatus.Pending &&
            x.NewStatus == ReportStatus.InProgress), It.IsAny<CancellationToken>()), Times.Once);

        _notifications.Verify(n => n.AddAsync(It.Is<Notification>(x =>
            x.UserId == owner && x.ReportId == report.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangeStatus_updates_priority_and_assignment()
    {
        var owner = Guid.NewGuid();
        var staff = Guid.NewGuid();
        var report = MakeReport(owner, ReportStatus.Pending);
        _reports.Setup(r => r.GetByIdAsync(report.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(report);
        _users.Setup(u => u.GetByIdAsync(staff, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = staff, FullName = "Staff", Role = UserRole.Staff, Email = "s@x.com", IsActive = true });
        var sut = Build();

        await sut.ChangeStatusAsync(report.Id,
            new ChangeReportStatusRequest
            {
                Status = ReportStatus.Assigned,
                Priority = Priority.High,
                AssignedToId = staff,
                Message = "Assigned to staff"
            },
            currentUserId: Guid.NewGuid());

        report.Priority.Should().Be(Priority.High);
        report.AssignedToId.Should().Be(staff);
    }

    [Fact]
    public async Task ChangeStatus_to_Resolved_sets_ResolvedAt()
    {
        var owner = Guid.NewGuid();
        var report = MakeReport(owner, ReportStatus.InProgress);
        _reports.Setup(r => r.GetByIdAsync(report.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(report);
        var sut = Build();

        await sut.ChangeStatusAsync(report.Id,
            new ChangeReportStatusRequest { Status = ReportStatus.Resolved },
            currentUserId: Guid.NewGuid());

        report.ResolvedAt.Should().NotBeNull();
        report.Status.Should().Be(ReportStatus.Resolved);
    }
}
