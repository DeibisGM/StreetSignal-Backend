using FluentAssertions;
using Moq;
using StreetSignalApi.Common.Enums;
using StreetSignalApi.Common.Exceptions;
using StreetSignalApi.DTOs.Requests;
using StreetSignalApi.Models;
using StreetSignalApi.Repositories.Interfaces;
using StreetSignalApi.Services.Implementations;
using StreetSignalApi.Services.Interfaces;

namespace StreetSignalApi.UnitTests.Services;

public class ReportUpdateServiceTests
{
    private readonly Mock<IReportRepository> _reports = new();
    private readonly Mock<IReportUpdateRepository> _updates = new();
    private readonly Mock<INotificationRepository> _notifications = new();
    private readonly Mock<IPushNotificationService> _push = new();

    private ReportUpdateService Build() => new(_reports.Object, _updates.Object, _notifications.Object, _push.Object);

    private static Report Report(Guid ownerId) => new()
    {
        Id = Guid.NewGuid(),
        Title = "T",
        Description = "D",
        CategoryId = Guid.NewGuid(),
        CreatedById = ownerId,
        Latitude = 1,
        Longitude = 1
    };

    [Fact]
    public async Task List_forbids_citizen_accessing_other_report()
    {
        var owner = Guid.NewGuid();
        var r = Report(owner);
        _reports.Setup(x => x.GetByIdAsync(r.Id, false, It.IsAny<CancellationToken>())).ReturnsAsync(r);

        var sut = Build();
        var act = () => sut.ListAsync(r.Id, Guid.NewGuid(), currentIsStaff: false);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Create_by_staff_on_other_user_report_notifies_citizen()
    {
        var owner = Guid.NewGuid();
        var staff = Guid.NewGuid();
        var r = Report(owner);
        _reports.Setup(x => x.GetByIdAsync(r.Id, false, It.IsAny<CancellationToken>())).ReturnsAsync(r);
        _updates.Setup(u => u.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReportUpdate
                {
                    Id = Guid.NewGuid(),
                    ReportId = r.Id,
                    CreatedById = staff,
                    CreatedBy = new User { Id = staff, FullName = "S", Role = UserRole.Staff, Email = "s@x.com" },
                    Type = ReportUpdateType.Comment,
                    Message = "hi"
                });

        var sut = Build();
        await sut.CreateAsync(r.Id, new CreateReportUpdateRequest { Message = "hi" }, staff, currentIsStaff: true);

        _notifications.Verify(n => n.AddAsync(It.Is<Notification>(x => x.UserId == owner && x.ReportId == r.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_by_citizen_on_own_report_does_not_notify()
    {
        var owner = Guid.NewGuid();
        var r = Report(owner);
        _reports.Setup(x => x.GetByIdAsync(r.Id, false, It.IsAny<CancellationToken>())).ReturnsAsync(r);
        _updates.Setup(u => u.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReportUpdate
                {
                    Id = Guid.NewGuid(),
                    ReportId = r.Id,
                    CreatedById = owner,
                    CreatedBy = new User { Id = owner, FullName = "O", Role = UserRole.Citizen, Email = "o@x.com" },
                    Type = ReportUpdateType.Comment,
                    Message = "self"
                });

        var sut = Build();
        await sut.CreateAsync(r.Id, new CreateReportUpdateRequest { Message = "self" }, owner, currentIsStaff: false);

        _notifications.Verify(n => n.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
