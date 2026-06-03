using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using StreetSignalApi.Common.Enums;
using StreetSignalApi.Configuration;
using StreetSignalApi.Models;
using StreetSignalApi.Services.Implementations;

namespace StreetSignalApi.UnitTests.Services;

public class JwtTokenServiceTests
{
    private static JwtTokenService Build()
    {
        var opts = Options.Create(new JwtOptions
        {
            Issuer = "test-issuer",
            Audience = "test-aud",
            SigningKey = "this-is-a-strong-test-signing-key-with-enough-bytes-123",
            ExpiresInSeconds = 3600
        });
        return new JwtTokenService(opts);
    }

    [Fact]
    public void GenerateToken_returns_token_with_expected_claims()
    {
        var sut = Build();
        var user = new User { Id = Guid.NewGuid(), Email = "x@y.com", FullName = "X Y", Role = UserRole.Staff };

        var (token, expiresIn) = sut.GenerateToken(user);

        expiresIn.Should().Be(3600);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Staff");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
        jwt.Issuer.Should().Be("test-issuer");
        jwt.Audiences.Should().Contain("test-aud");
    }
}
