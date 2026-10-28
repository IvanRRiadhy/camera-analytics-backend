using System.Text;
using CCTV.Infrastructure.Security.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace CCTV.UnitTests.Identity;

public class JwtConfigurationTests
{
    [Fact]
    public void TokenValidationParameters_MatchesJwtOptions()
    {
        var options = new JwtOptions
        {
            SecretKey = "SuperSecretKeyForCctvAnalyticsTestingJwtTokensMustBeAtLeast32BytesLong!",
            Issuer = "CCTV.Analytics",
            Audience = "CCTV.Clients",
            ExpiryMinutes = 1440
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = options.Issuer,
            ValidateAudience = true,
            ValidAudience = options.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        Assert.NotNull(validationParams.IssuerSigningKey);
        Assert.Equal("CCTV.Analytics", validationParams.ValidIssuer);
        Assert.Equal("CCTV.Clients", validationParams.ValidAudience);
        Assert.True(validationParams.ValidateLifetime);
    }
}
