using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StreetSignalApi.DTOs.Responses;

namespace StreetSignalApi.IntegrationTests.Endpoints;

public class HealthEndpointsTests : IClassFixture<StreetSignalWebAppFactory>
{
    private readonly StreetSignalWebAppFactory _factory;
    public HealthEndpointsTests(StreetSignalWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task GET_health_returns_ok_without_auth()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/health");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<HealthResponse>();
        body!.Status.Should().Be("ok");
    }
}
