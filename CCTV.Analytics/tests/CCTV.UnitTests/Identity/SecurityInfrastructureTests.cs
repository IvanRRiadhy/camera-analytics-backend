using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CCTV.Domain.Entities.Identity;
using CCTV.Infrastructure.Security;
using CCTV.Infrastructure.Security.Options;
using Microsoft.Extensions.Options;
using Xunit;

namespace CCTV.UnitTests.Identity;

public class SecurityInfrastructureTests
{
    private readonly PasswordHasher _hasher = new();
    private readonly JwtTokenGenerator _tokenGenerator;

    public SecurityInfrastructureTests()
    {
        var options = Options.Create(new JwtOptions
        {
            SecretKey = "SuperSecretKeyForCctvAnalyticsTestingJwtTokensMustBeAtLeast32BytesLong!",
            Issuer = "CCTV.Analytics",
            Audience = "CCTV.Clients",
            ExpiryMinutes = 1440
        });
        _tokenGenerator = new JwtTokenGenerator(options);
    }

    [Fact]
    public void PasswordHasher_HashAndVerify_SucceedsForMatchingPassword()
    {
        var password = "SecurePassword123!";
        var hash = _hasher.HashPassword(password);

        Assert.NotEmpty(hash);
        Assert.NotEqual(password, hash);
        Assert.True(_hasher.VerifyPassword(password, hash));
        Assert.False(_hasher.VerifyPassword("WrongPassword", hash));
    }

    [Fact]
    public void JwtTokenGenerator_GeneratesValidToken_WithRequiredClaims()
    {
        var tenantId = Guid.NewGuid();
        var user = new User
        {
            ApplicationId = tenantId,
            Username = "janedoe",
            Email = "user@tenant.com",
            FullName = "Jane Doe"
        };
        var userId = user.Id;
        var roles = new List<string> { "Admin", "Operator" };

        var (tokenString, expiresIn) = _tokenGenerator.GenerateToken(user, roles);

        Assert.NotEmpty(tokenString);
        Assert.Equal(86400, expiresIn); // 1440 minutes * 60 seconds

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);

        Assert.Equal(userId.ToString(), token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub || c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal(tenantId.ToString(), token.Claims.First(c => c.Type == "app_id").Value);
        Assert.Equal(tenantId.ToString(), token.Claims.First(c => c.Type == "tenant_id").Value);
        Assert.Equal("user@tenant.com", token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == "email").Value);
        Assert.Equal(1, token.Claims.Count(c => c.Type == "email" || c.Type == JwtRegisteredClaimNames.Email));
        Assert.Equal("Jane Doe", token.Claims.First(c => c.Type == "fullname").Value);
        Assert.Equal("janedoe", token.Claims.First(c => c.Type == "username").Value);
        Assert.Contains(token.Claims, c => (c.Type == "role" || c.Type == ClaimTypes.Role) && c.Value == "Admin");
    }
}
