using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StreetSignalApi.DTOs.Requests;
using StreetSignalApi.DTOs.Responses;

namespace StreetSignalApi.IntegrationTests.Endpoints;

public class AuthEndpointsTests : IClassFixture<StreetSignalWebAppFactory>
{
    private readonly StreetSignalWebAppFactory _factory;
    public AuthEndpointsTests(StreetSignalWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task Login_with_seeded_citizen_returns_token()
    {
        var client = _factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = StreetSignalWebAppFactory.CitizenEmail,
            Password = StreetSignalWebAppFactory.Password
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        body!.Token.Should().NotBeNullOrWhiteSpace();
        body.User.Email.Should().Be(StreetSignalWebAppFactory.CitizenEmail);
    }

    [Fact]
    public async Task Login_with_bad_password_returns_401_INVALID_CREDENTIALS()
    {
        var client = _factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = StreetSignalWebAppFactory.CitizenEmail,
            Password = "WrongPass!"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var body = await resp.Content.ReadFromJsonAsync<ErrorResponse>();
        body!.Code.Should().Be("INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task Register_with_existing_email_returns_409_EMAIL_ALREADY_EXISTS()
    {
        var client = _factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FullName = "Dup", Email = StreetSignalWebAppFactory.CitizenEmail, Password = "Password123!"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await resp.Content.ReadFromJsonAsync<ErrorResponse>();
        body!.Code.Should().Be("EMAIL_ALREADY_EXISTS");
    }

    [Fact]
    public async Task Register_with_invalid_payload_returns_400_VALIDATION_ERROR()
    {
        var client = _factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FullName = "x", Email = "not-an-email", Password = "123"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadFromJsonAsync<ValidationErrorResponse>();
        body!.Code.Should().Be("VALIDATION_ERROR");
        body.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Me_without_token_returns_401_UNAUTHORIZED()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/auth/me");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var body = await resp.Content.ReadFromJsonAsync<ErrorResponse>();
        body!.Code.Should().Be("UNAUTHORIZED");
    }

    [Fact]
    public async Task Me_with_token_returns_user_profile()
    {
        var client = await _factory.AuthedAsync(StreetSignalWebAppFactory.CitizenEmail);
        var resp = await client.GetAsync("/api/auth/me");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<UserDto>();
        body!.Email.Should().Be(StreetSignalWebAppFactory.CitizenEmail);
    }

    [Fact]
    public async Task Logout_returns_204()
    {
        var client = await _factory.AuthedAsync(StreetSignalWebAppFactory.CitizenEmail);
        var resp = await client.PostAsync("/api/auth/logout", content: null);
        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
