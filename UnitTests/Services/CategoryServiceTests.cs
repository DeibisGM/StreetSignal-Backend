using FluentAssertions;
using Moq;
using StreetSignalApi.Models;
using StreetSignalApi.Repositories.Interfaces;
using StreetSignalApi.Services.Implementations;

namespace StreetSignalApi.UnitTests.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repo = new();

    [Fact]
    public async Task NonStaff_request_with_includeInactive_true_is_forced_to_false()
    {
        _repo.Setup(r => r.ListAsync(false, It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Category> { new() { Name = "Active", IsActive = true } });

        var sut = new CategoryService(_repo.Object);

        var result = await sut.ListAsync(includeInactive: true, requesterIsStaff: false);

        result.Data.Should().HaveCount(1);
        _repo.Verify(r => r.ListAsync(false, It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.ListAsync(true,  It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Staff_request_with_includeInactive_true_is_passed_through()
    {
        _repo.Setup(r => r.ListAsync(true, It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Category>
             {
                 new() { Name = "Active",   IsActive = true },
                 new() { Name = "Inactive", IsActive = false }
             });

        var sut = new CategoryService(_repo.Object);

        var result = await sut.ListAsync(includeInactive: true, requesterIsStaff: true);

        result.Data.Should().HaveCount(2);
        _repo.Verify(r => r.ListAsync(true, It.IsAny<CancellationToken>()), Times.Once);
    }
}
