using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreetSignalApi.Common.Services;
using StreetSignalApi.DTOs.Responses;
using StreetSignalApi.Services.Interfaces;

namespace StreetSignalApi.Controllers;

[ApiController]
[Route("api/categories")]
[AllowAnonymous]
public sealed class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categories;
    private readonly ICurrentUserService _current;

    public CategoriesController(ICategoryService categories, ICurrentUserService current)
    {
        _categories = categories;
        _current = current;
    }

    [HttpGet]
    [ProducesResponseType(typeof(CategoryListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CategoryListResponse>> List(
        [FromQuery] bool includeInactive = false,
        CancellationToken ct = default)
    {
        var isStaff = _current.IsAuthenticated && _current.IsStaffOrAdmin;
        return Ok(await _categories.ListAsync(includeInactive, isStaff, ct));
    }
}
