using FluentAssertions;
using Moq;
using StreetSignalApi.Common.Exceptions;
using StreetSignalApi.Models;
using StreetSignalApi.Repositories.Interfaces;
using StreetSignalApi.Services.Implementations;

namespace StreetSignalApi.UnitTests.Services;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _notifs = new();
    private readonly Mock<IDeviceTokenRepository> _tokens = new();

    private NotificationService Build() => new(_notifs.Object, _tokens.Object);

    [Fact]
    public async Task MarkAsRead_throws_NotFound_when_missing()
    {
        _notifs.Setup(n => n.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Notification?)null);
        var sut = Build();

        var act = () => sut.MarkAsReadAsync(Guid.NewGuid(), Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task MarkAsRead_throws_Forbidden_when_wrong_user()
    {
        var ownerId = Guid.NewGuid();
        var n = new Notification { Id = Guid.NewGuid(), UserId = ownerId, IsRead = false };
        _notifs.Setup(r => r.GetByIdAsync(n.Id, It.IsAny<CancellationToken>())).ReturnsAsync(n);
        var sut = Build();

        var act = () => sut.MarkAsReadAsync(n.Id, Guid.NewGuid());

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task MarkAsRead_marks_and_saves()
    {
        var owner = Guid.NewGuid();
        var n = new Notification { Id = Guid.NewGuid(), UserId = owner, IsRead = false };
        _notifs.Setup(r => r.GetByIdAsync(n.Id, It.IsAny<CancellationToken>())).ReturnsAsync(n);
        var sut = Build();

        await sut.MarkAsReadAsync(n.Id, owner);

        n.IsRead.Should().BeTrue();
        _notifs.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task List_clamps_pageSize_above_100()
    {
        _notifs.Setup(r => r.ListForUserAsync(It.IsAny<Guid>(), false, 1, 100, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new PagedResult<Notification>(new List<Notification>(), 0));
        var sut = Build();

        await sut.ListAsync(Guid.NewGuid(), unreadOnly: false, page: 1, pageSize: 5000);

        _notifs.Verify(r => r.ListForUserAsync(It.IsAny<Guid>(), false, 1, 100, It.IsAny<CancellationToken>()), Times.Once);
    }
}
