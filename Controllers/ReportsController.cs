using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreetSignalApi.Common.Enums;
using StreetSignalApi.Common.Services;
using StreetSignalApi.DTOs.Requests;
using StreetSignalApi.DTOs.Responses;
using StreetSignalApi.Permissions;
using StreetSignalApi.Services.Interfaces;

namespace StreetSignalApi.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportService _reports;
    private readonly ICurrentUserService _current;

    public ReportsController(IReportService reports, ICurrentUserService current)
    {
        _reports = reports;
        _current = current;
    }

    [HttpGet]
    [Authorize(Policy = Policies.StaffOnly)]
    [ProducesResponseType(typeof(PaginatedReportListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginatedReportListResponse>> ListForStaff(
        [FromQuery] ReportStatus? status,
        [FromQuery] Guid? categoryId,
        [FromQuery] string? search,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _reports.ListForStaffAsync(status, categoryId, search, fromDate, toDate, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("my")]
    [ProducesResponseType(typeof(PaginatedReportListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginatedReportListResponse>> ListMine(
        [FromQuery] ReportStatus? status,
        [FromQuery] Guid? categoryId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var result = await _reports.ListMyAsync(userId, status, categoryId, search, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{reportId:guid}")]
    [ProducesResponseType(typeof(ReportDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportDetailResponse>> GetById(Guid reportId, CancellationToken ct)
    {
        var userId = _current.RequireUserId();
        var result = await _reports.GetByIdAsync(reportId, userId, _current.IsStaffOrAdmin, ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReportDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ReportDetailResponse>> Create([FromBody] CreateReportRequest req, CancellationToken ct)
    {
        var userId = _current.RequireUserId();
        var result = await _reports.CreateAsync(req, userId, ct);
        var location = Url.Action(nameof(GetById), new { reportId = result.Data.Id }) ?? $"/api/reports/{result.Data.Id}";
        return Created(location, result);
    }

    [HttpPatch("{reportId:guid}")]
    [ProducesResponseType(typeof(ReportDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReportDetailResponse>> Update(Guid reportId, [FromBody] UpdateReportRequest req, CancellationToken ct)
    {
        var userId = _current.RequireUserId();
        var result = await _reports.UpdateAsync(reportId, req, userId, ct);
        return Ok(result);
    }

    [HttpPatch("{reportId:guid}/status")]
    [Authorize(Policy = Policies.StaffOnly)]
    [ProducesResponseType(typeof(ReportDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportDetailResponse>> ChangeStatus(Guid reportId, [FromBody] ChangeReportStatusRequest req, CancellationToken ct)
    {
        var userId = _current.RequireUserId();
        var result = await _reports.ChangeStatusAsync(reportId, req, userId, ct);
        return Ok(result);
    }
}
