using StreetSignalApi.DTOs.Responses;
using StreetSignalApi.Models;

namespace StreetSignalApi.Mappers;

public static class UserMapper
{
    public static UserDto ToDto(this User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Phone = user.Phone,
        Role = user.Role,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt
    };

    public static UserBasicDto ToBasicDto(this User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Role = user.Role
    };
}
