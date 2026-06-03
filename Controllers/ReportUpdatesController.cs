using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreetSignalApi.Common.Services;
using StreetSignalApi.DTOs.Requests;
using StreetSignalApi.DTOs.Responses;
using StreetSignalApi.Services.Interfaces;

namespace StreetSignalApi.Controllers;

[ApiController]
[Route("api/reports/{reportId:guid}/updates")]
[Authorize]
public sealed class ReportUpdatesController : ControllerBase
{
    private readonly IReportUpdateService _updates;
    private readonly ICurrentUserService _current;

    public ReportUpdatesController(IReportUpdateService updates, ICurrentUserService current)
    {
        _updates = updates;
        _current = current;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ReportUpdateListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportUpdateListResponse>> List(Guid reportId, CancellationToken ct)
    {
        var userId = _current.RequireUserId();
        var result = await _updates.ListAsync(reportId, userId, _current.IsStaffOrAdmin, ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReportUpdateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportUpdateResponse>> Create(Guid reportId, [FromBody] CreateReportUpdateRequest req, CancellationToken ct)
    {
        var userId = _current.RequireUserId();
        var result = await _updates.CreateAsync(reportId, req, userId, _current.IsStaffOrAdmin, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}
