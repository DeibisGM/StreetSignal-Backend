using StreetSignalApi.Models;

namespace StreetSignalApi.Services.Interfaces;

public interface IJwtTokenService
{
    (string Token, int ExpiresInSeconds) GenerateToken(User user);
}
