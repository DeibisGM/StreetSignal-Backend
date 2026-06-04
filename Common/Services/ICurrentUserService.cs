using StreetSignalApi.Common.Enums;

namespace StreetSignalApi.Common.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
    bool IsStaffOrAdmin { get; }
    Guid RequireUserId();
}
