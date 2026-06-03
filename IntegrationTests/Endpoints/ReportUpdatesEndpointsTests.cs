using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StreetSignalApi.DTOs.Requests;
using StreetSignalApi.DTOs.Responses;

namespace StreetSignalApi.IntegrationTests.Endpoints;

public class ReportUpdatesEndpointsTests : IClassFixture<StreetSignalWebAppFactory>
{
    private readonly StreetSignalWebAppFactory _factory;
    public ReportUpdatesEndpointsTests(StreetSignalWebAppFactory factory) => _factory = factory;

    private async Task<Guid> CreateReportAsync(HttpClient client)
    {
        var resp = await client.PostAsJsonAsync("/api/reports", new CreateReportRequest
        {
            Title = "Test report for comments",
            Description = "Description long enough.",
            CategoryId = _factory.CategoryId,
            Latitude = 10.32, Longitude = -84.43
        });
        resp.EnsureSuccessStatusCode();
        var detail = await resp.Content.ReadFromJsonAsync<ReportDetailResponse>();
        return detail!.Data.Id;
    }

    [Fact]
    public async Task Citizen_can_list_and_add_updates_to_own_report()
    {
        var client = await _factory.AuthedAsync(StreetSignalWebAppFactory.CitizenEmail);
        var reportId = await CreateReportAsync(client);

        var post = await client.PostAsJsonAsync($"/api/reports/{reportId}/updates",
            new CreateReportUpdateRequest { Message = "Adding more context here." });
        post.StatusCode.Should().Be(HttpStatusCode.Created);

        var list = await client.GetAsync($"/api/reports/{reportId}/updates");
        list.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await list.Content.ReadFromJsonAsync<ReportUpdateListResponse>();
        body!.Data.Should().Contain(u => u.Message == "Adding more context here.");
    }

    [Fact]
    public async Task Staff_can_comment_on_any_report_and_creates_notification()
    {
        var citizenClient = await _factory.AuthedAsync(StreetSignalWebAppFactory.CitizenEmail);
        var reportId = await CreateReportAsync(citizenClient);

        var staffClient = await _factory.AuthedAsync(StreetSignalWebAppFactory.StaffEmail);
        var post = await staffClient.PostAsJsonAsync($"/api/reports/{reportId}/updates",
            new CreateReportUpdateRequest { Message = "Inspecting tomorrow morning." });
        post.StatusCode.Should().Be(HttpStatusCode.Created);

        // Citizen sees a new notification
        var notifs = await citizenClient.GetAsync("/api/notifications?unreadOnly=true");
        notifs.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await notifs.Content.ReadFromJsonAsync<PaginatedNotificationListResponse>();
        body!.Data.Should().Contain(n => n.ReportId == reportId);
    }
}
