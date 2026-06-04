using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StreetSignalApi.Common.Enums;
using StreetSignalApi.DTOs.Requests;
using StreetSignalApi.DTOs.Responses;

namespace StreetSignalApi.IntegrationTests.Endpoints;

public class ReportsEndpointsTests : IClassFixture<StreetSignalWebAppFactory>
{
    private readonly StreetSignalWebAppFactory _factory;
    public ReportsEndpointsTests(StreetSignalWebAppFactory factory) => _factory = factory;

    private CreateReportRequest ValidReport() => new()
    {
        Title = "Pothole in Main Street",
        Description = "Large pothole, dangerous for vehicles.",
        CategoryId = _factory.CategoryId,
        Latitude = 10.32,
        Longitude = -84.43,
        Address = "Main Street"
    };

    [Fact]
    public async Task Citizen_can_create_report_and_GET_it()
    {
        var client = await _factory.AuthedAsync(StreetSignalWebAppFactory.CitizenEmail);

        var createResp = await client.PostAsJsonAsync("/api/reports", ValidReport());
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResp.Content.ReadFromJsonAsync<ReportDetailResponse>();
        created!.Data.Status.Should().Be(ReportStatus.Pending);

        var getResp = await client.GetAsync($"/api/reports/{created.Data.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Citizen_cannot_call_staff_list()
    {
        var client = await _factory.AuthedAsync(StreetSignalWebAppFactory.CitizenEmail);
        var resp = await client.GetAsync("/api/reports");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var body = await resp.Content.ReadFromJsonAsync<ErrorResponse>();
        body!.Code.Should().Be("FORBIDDEN");
    }

    [Fact]
    public async Task Staff_can_call_list()
    {
        var client = await _factory.AuthedAsync(StreetSignalWebAppFactory.StaffEmail);
        var resp = await client.GetAsync("/api/reports?page=1&pageSize=20");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<PaginatedReportListResponse>();
        body!.Pagination.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task Citizen_can_list_my_reports()
    {
        var client = await _factory.AuthedAsync(StreetSignalWebAppFactory.CitizenEmail);
        await client.PostAsJsonAsync("/api/reports", ValidReport());

        var resp = await client.GetAsync("/api/reports/my");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<PaginatedReportListResponse>();
        body!.Data.Should().NotBeEmpty();
        body.Data.Should().OnlyContain(r => r.CreatedBy.Id == _factory.CitizenId);
    }

    [Fact]
    public async Task Citizen_cannot_view_other_citizen_report()
    {
        // First citizen creates a report
        var ownerClient = await _factory.AuthedAsync(StreetSignalWebAppFactory.CitizenEmail);
        var createResp = await ownerClient.PostAsJsonAsync("/api/reports", ValidReport());
        var created = await createResp.Content.ReadFromJsonAsync<ReportDetailResponse>();

        // Register a brand-new citizen and try to read the first citizen's report
        var anonClient = _factory.CreateClient();
        var newEmail = $"another-{Guid.NewGuid():N}@test.com";
        var registerResp = await anonClient.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FullName = "Other Citizen",
            Email = newEmail,
            Password = "Password123!"
        });
        registerResp.EnsureSuccessStatusCode();
        var auth = await registerResp.Content.ReadFromJsonAsync<AuthResponse>();

        anonClient.DefaultRequestHeaders.Authorization = new("Bearer", auth!.Token);
        var resp = await anonClient.GetAsync($"/api/reports/{created!.Data.Id}");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_with_invalid_payload_returns_400()
    {
        var client = await _factory.AuthedAsync(StreetSignalWebAppFactory.CitizenEmail);
        var resp = await client.PostAsJsonAsync("/api/reports", new CreateReportRequest
        {
            Title = "x",
            Description = "short",
            Latitude = 999,
            Longitude = 999
        });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadFromJsonAsync<ValidationErrorResponse>();
        body!.Code.Should().Be("VALIDATION_ERROR");
        body.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Staff_can_change_status_and_creates_timeline_entry()
    {
        // Citizen creates a report
        var citizenClient = await _factory.AuthedAsync(StreetSignalWebAppFactory.CitizenEmail);
        var create = await citizenClient.PostAsJsonAsync("/api/reports", ValidReport());
        var created = await create.Content.ReadFromJsonAsync<ReportDetailResponse>();

        // Staff changes the status
        var staffClient = await _factory.AuthedAsync(StreetSignalWebAppFactory.StaffEmail);
        var patch = await staffClient.PatchAsJsonAsync($"/api/reports/{created!.Data.Id}/status",
            new ChangeReportStatusRequest { Status = ReportStatus.InProgress, Message = "Working on it" });

        patch.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await patch.Content.ReadFromJsonAsync<ReportDetailResponse>();
        detail!.Data.Status.Should().Be(ReportStatus.InProgress);
        detail.Data.Updates.Should().Contain(u => u.Type == ReportUpdateType.StatusChange);
    }

    [Fact]
    public async Task Citizen_cannot_update_non_pending_report_returns_409_REPORT_NOT_EDITABLE()
    {
        var citizenClient = await _factory.AuthedAsync(StreetSignalWebAppFactory.CitizenEmail);
        var create = await citizenClient.PostAsJsonAsync("/api/reports", ValidReport());
        var created = await create.Content.ReadFromJsonAsync<ReportDetailResponse>();

        // Staff moves it out of Pending
        var staffClient = await _factory.AuthedAsync(StreetSignalWebAppFactory.StaffEmail);
        await staffClient.PatchAsJsonAsync($"/api/reports/{created!.Data.Id}/status",
            new ChangeReportStatusRequest { Status = ReportStatus.InReview });

        // Citizen tries to edit
        var patch = await citizenClient.PatchAsJsonAsync($"/api/reports/{created.Data.Id}",
            new UpdateReportRequest { Title = "Updated title here" });

        patch.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await patch.Content.ReadFromJsonAsync<ErrorResponse>();
        body!.Code.Should().Be("REPORT_NOT_EDITABLE");
    }

    [Fact]
    public async Task GetById_unknown_returns_404()
    {
        var client = await _factory.AuthedAsync(StreetSignalWebAppFactory.StaffEmail);
        var resp = await client.GetAsync($"/api/reports/{Guid.NewGuid()}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await resp.Content.ReadFromJsonAsync<ErrorResponse>();
        body!.Code.Should().Be("NOT_FOUND");
    }
}
