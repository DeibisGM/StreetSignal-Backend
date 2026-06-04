using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StreetSignalApi.DTOs.Responses;

namespace StreetSignalApi.IntegrationTests.Endpoints;

public class CategoriesEndpointsTests : IClassFixture<StreetSignalWebAppFactory>
{
    private readonly StreetSignalWebAppFactory _factory;
    public CategoriesEndpointsTests(StreetSignalWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task List_without_auth_returns_only_active()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/categories?includeInactive=true");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<CategoryListResponse>();
        body!.Data.Should().OnlyContain(c => c.IsActive);
    }

    [Fact]
    public async Task List_with_staff_token_and_includeInactive_returns_inactive_too()
    {
        var client = await _factory.AuthedAsync(StreetSignalWebAppFactory.StaffEmail);
        var resp = await client.GetAsync("/api/categories?includeInactive=true");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<CategoryListResponse>();
        body!.Data.Should().Contain(c => !c.IsActive);
    }
}
