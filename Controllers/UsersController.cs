using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreetSignalApi.DTOs.Responses;
using StreetSignalApi.Mappers;
using StreetSignalApi.Permissions;
using StreetSignalApi.Repositories.Interfaces;

namespace StreetSignalApi.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = Policies.StaffOnly)]
public sealed class UsersController : ControllerBase
{
    private readonly IUserRepository _users;

    public UsersController(IUserRepository users)
    {
        _users = users;
    }

    [HttpGet("staff")]
    [ProducesResponseType(typeof(List<UserBasicDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserBasicDto>>> ListStaff(CancellationToken ct)
    {
        var staff = await _users.ListStaffAsync(ct);
        return Ok(staff.Select(u => u.ToBasicDto()).ToList());
    }
}
