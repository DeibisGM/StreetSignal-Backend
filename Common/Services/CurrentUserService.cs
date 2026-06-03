using System.Security.Claims;
using StreetSignalApi.Common.Enums;
using StreetSignalApi.Common.Exceptions;
using StreetSignalApi.Common.Errors;

namespace StreetSignalApi.Common.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var raw = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User?.FindFirstValue("sub");
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    public UserRole? Role
    {
        get
        {
            var raw = User?.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<UserRole>(raw, ignoreCase: true, out var r) ? r : null;
        }
    }

    public bool IsStaffOrAdmin => Role is UserRole.Staff or UserRole.Admin;

    public Guid RequireUserId()
    {
        return UserId ?? throw new UnauthorizedException(ErrorCodes.Unauthorized, "Authentication is required.");
    }
}
