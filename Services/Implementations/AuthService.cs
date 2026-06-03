using Microsoft.AspNetCore.Identity;
using StreetSignalApi.Common.Errors;
using StreetSignalApi.Common.Exceptions;
using StreetSignalApi.DTOs.Requests;
using StreetSignalApi.DTOs.Responses;
using StreetSignalApi.Mappers;
using StreetSignalApi.Models;
using StreetSignalApi.Repositories.Interfaces;
using StreetSignalApi.Services.Interfaces;

namespace StreetSignalApi.Services.Implementations;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IJwtTokenService _jwt;

    public AuthService(IUserRepository users, IPasswordHasher<User> hasher, IJwtTokenService jwt)
    {
        _users = users;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var normalizedEmail = request.Email.Trim().ToLower();

        if (await _users.EmailExistsAsync(normalizedEmail, ct))
        {
            throw new ConflictException(ErrorCodes.EmailAlreadyExists, "The provided email is already registered.");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            Phone = request.Phone,
            Role = Common.Enums.UserRole.Citizen,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        await _users.AddAsync(user, ct);
        await _users.SaveChangesAsync(ct);

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(request.Email.Trim().ToLower(), ct);
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedException(ErrorCodes.InvalidCredentials, "Invalid email or password.");
        }

        PasswordVerificationResult result;
        try
        {
            result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        }
        catch (FormatException)
        {
            // Stored hash is not valid Base-64 (e.g. manually inserted plain-text password).
            throw new UnauthorizedException(ErrorCodes.InvalidCredentials, "Invalid email or password.");
        }

        if (result == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedException(ErrorCodes.InvalidCredentials, "Invalid email or password.");
        }

        return BuildAuthResponse(user);
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct)
            ?? throw new UnauthorizedException(ErrorCodes.Unauthorized, "Authentication is required.");
        return user.ToDto();
    }

    private AuthResponse BuildAuthResponse(User user)
    {
        var (token, expires) = _jwt.GenerateToken(user);
        return new AuthResponse
        {
            Token = token,
            TokenType = "Bearer",
            ExpiresIn = expires,
            User = user.ToDto()
        };
    }
}
