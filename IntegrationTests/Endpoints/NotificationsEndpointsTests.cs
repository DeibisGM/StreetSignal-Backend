using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StreetSignalApi.Common.Enums;
using StreetSignalApi.DTOs.Requests;
using StreetSignalApi.DTOs.Responses;

namespace StreetSignalApi.IntegrationTests.Endpoints;

public class NotificationsEndpointsTests : IClassFixture<StreetSignalWebAppFactory>
{
    private readonly StreetSignalWebAppFactory _factory;
    public NotificationsEndpointsTests(StreetSignalWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task Register_device_token_returns_204()
    {
        var client = await _factory.AuthedAsync(StreetSignalWebAppFactory.CitizenEmail);
        var resp = await client.PostAsJsonAsync("/api/notifications/device-token",
            new RegisterDeviceTokenRequest { Token = "ExponentPushToken[xxxxxxxxxx]", Platform = Platform.Android });
        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Mark_unknown_notification_returns_404()
    {
        var client = await _factory.AuthedAsync(StreetSignalWebAppFactory.CitizenEmail);
        var resp = await client.PatchAsync($"/api/notifications/{Guid.NewGuid()}/read", content: null);
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task List_without_token_returns_401()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/notifications");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var body = await resp.Content.ReadFromJsonAsync<ErrorResponse>();
        body!.Code.Should().Be("UNAUTHORIZED");
    }
}
