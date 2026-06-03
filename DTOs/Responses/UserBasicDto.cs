using StreetSignalApi.Common.Enums;

namespace StreetSignalApi.DTOs.Responses;

public class UserBasicDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
