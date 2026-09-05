using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using TaskManagement.Server.Common;
using TaskManagement.Server.Entities;
using TaskManagement.Server.Services;
using Xunit;

namespace TaskManagement.Tests;

public class AuthTests
{
    [Fact]
    public void BCrypt_ShouldCorrectlyHash_AndVerifyPassword()
    {
        // Arrange
        const string password = "SecretPassword@123";

        // Act
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        var isValid = BCrypt.Net.BCrypt.Verify(password, hash);
        var isInvalid = BCrypt.Net.BCrypt.Verify("WrongPassword", hash);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotBe(password);
        isValid.Should().BeTrue();
        isInvalid.Should().BeFalse();
    }

    [Fact]
    public void TokenService_ShouldGenerateValidJwt_WithRequiredClaims()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:Key", "VeryLongSecretKeyForUnitTestingJwtTokens2026!MustBe32Bytes" },
            { "Jwt:Issuer", "TaskManagementTestIssuer" },
            { "Jwt:Audience", "TaskManagementTestAudience" },
            { "Jwt:ExpiryInHours", "2" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var tokenService = new TokenService(configuration);

        var testUser = new User
        {
            Id = 42,
            FullName = "Jane Doe",
            Email = "jane.doe@example.com",
            Role = UserRole.Manager
        };

        // Act
        var (token, expiresAt) = tokenService.GenerateToken(testUser);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        expiresAt.Should().BeAfter(DateTime.UtcNow);

        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();

        var jwt = handler.ReadJwtToken(token);
        jwt.Issuer.Should().Be("TaskManagementTestIssuer");
        jwt.Audiences.Should().Contain("TaskManagementTestAudience");

        var claims = jwt.Claims.ToList();
        claims.Should().Contain(c => (c.Type == "nameid" || c.Type == ClaimTypes.NameIdentifier) && c.Value == "42");
        claims.Should().Contain(c => (c.Type == "email" || c.Type == ClaimTypes.Email) && c.Value == "jane.doe@example.com");
        claims.Should().Contain(c => (c.Type == "unique_name" || c.Type == ClaimTypes.Name) && c.Value == "Jane Doe");
        claims.Should().Contain(c => (c.Type == "role" || c.Type == ClaimTypes.Role) && c.Value == "Manager");
    }
}
