using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using StreetSignalApi.Common.Errors;
using StreetSignalApi.Common.Exceptions;
using StreetSignalApi.DTOs.Requests;
using StreetSignalApi.Models;
using StreetSignalApi.Repositories.Interfaces;
using StreetSignalApi.Services.Implementations;
using StreetSignalApi.Services.Interfaces;

namespace StreetSignalApi.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher<User>> _hasher = new();
    private readonly Mock<IJwtTokenService> _jwt = new();

    private AuthService Build()
    {
        _jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns(("test-token", 3600));
        return new AuthService(_users.Object, _hasher.Object, _jwt.Object);
    }

    [Fact]
    public async Task Register_succeeds_when_email_is_new()
    {
        _users.Setup(u => u.EmailExistsAsync("new@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _hasher.Setup(h => h.HashPassword(It.IsAny<User>(), "Password123!")).Returns("HASHED");
        var sut = Build();

        var result = await sut.RegisterAsync(new RegisterRequest
        {
            FullName = "Alice", Email = "New@Example.com", Password = "Password123!"
        });

        result.Token.Should().Be("test-token");
        result.ExpiresIn.Should().Be(3600);
        result.User.Email.Should().Be("new@example.com");
        _users.Verify(u => u.AddAsync(It.Is<User>(x => x.Email == "new@example.com" && x.PasswordHash == "HASHED"), It.IsAny<CancellationToken>()), Times.Once);
        _users.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_throws_conflict_when_email_exists()
    {
        _users.Setup(u => u.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = Build();

        var act = () => sut.RegisterAsync(new RegisterRequest
        {
            FullName = "Bob", Email = "taken@example.com", Password = "Password123!"
        });

        var ex = await act.Should().ThrowAsync<ConflictException>();
        ex.Which.Code.Should().Be(ErrorCodes.EmailAlreadyExists);
    }

    [Fact]
    public async Task Login_throws_unauthorized_when_user_not_found()
    {
        _users.Setup(u => u.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var sut = Build();

        var act = () => sut.LoginAsync(new LoginRequest { Email = "nobody@x.com", Password = "xx" });

        var ex = await act.Should().ThrowAsync<UnauthorizedException>();
        ex.Which.Code.Should().Be(ErrorCodes.InvalidCredentials);
    }

    [Fact]
    public async Task Login_throws_unauthorized_when_inactive()
    {
        var u = new User { Email = "x@x.com", PasswordHash = "h", IsActive = false };
        _users.Setup(r => r.GetByEmailAsync("x@x.com", It.IsAny<CancellationToken>())).ReturnsAsync(u);
        var sut = Build();

        var act = () => sut.LoginAsync(new LoginRequest { Email = "x@x.com", Password = "p" });

        var ex = await act.Should().ThrowAsync<UnauthorizedException>();
        ex.Which.Code.Should().Be(ErrorCodes.InvalidCredentials);
    }

    [Fact]
    public async Task Login_throws_unauthorized_when_password_wrong()
    {
        var u = new User { Email = "x@x.com", PasswordHash = "h", IsActive = true };
        _users.Setup(r => r.GetByEmailAsync("x@x.com", It.IsAny<CancellationToken>())).ReturnsAsync(u);
        _hasher.Setup(h => h.VerifyHashedPassword(u, "h", "bad")).Returns(PasswordVerificationResult.Failed);
        var sut = Build();

        var act = () => sut.LoginAsync(new LoginRequest { Email = "x@x.com", Password = "bad" });

        var ex = await act.Should().ThrowAsync<UnauthorizedException>();
        ex.Which.Code.Should().Be(ErrorCodes.InvalidCredentials);
    }

    [Fact]
    public async Task Login_succeeds_with_correct_password()
    {
        var u = new User { Id = Guid.NewGuid(), Email = "x@x.com", PasswordHash = "h", IsActive = true, FullName = "X" };
        _users.Setup(r => r.GetByEmailAsync("x@x.com", It.IsAny<CancellationToken>())).ReturnsAsync(u);
        _hasher.Setup(h => h.VerifyHashedPassword(u, "h", "p")).Returns(PasswordVerificationResult.Success);
        var sut = Build();

        var result = await sut.LoginAsync(new LoginRequest { Email = "x@x.com", Password = "p" });

        result.Token.Should().Be("test-token");
        result.User.Id.Should().Be(u.Id);
    }
}
