using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CCTV.Application.Identity.Interfaces;
using CCTV.Domain.Entities.Identity;
using CCTV.Infrastructure.Security.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CCTV.Infrastructure.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            throw new InvalidOperationException("Jwt:SecretKey is required in appsettings.json and cannot be empty.");
        }

        if (_options.ExpiryMinutes <= 0)
        {
            throw new InvalidOperationException("Jwt:ExpiryMinutes is required in appsettings.json and must be greater than zero.");
        }
    }

    public (string Token, int ExpiresInSeconds) GenerateToken(User user, IList<string> roles, IList<string>? permissions = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("username", user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("fullname", user.FullName),
            new(JwtRegisteredClaimNames.Name, user.FullName),
            new("app_id", user.ApplicationId.ToString()),
            new("tenant_id", user.ApplicationId.ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        if (permissions != null)
        {
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = string.IsNullOrWhiteSpace(_options.Issuer) ? null : _options.Issuer,
            Audience = string.IsNullOrWhiteSpace(_options.Audience) ? null : _options.Audience,
            SigningCredentials = creds
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        var tokenString = handler.WriteToken(token);
        var expiresInSeconds = _options.ExpiryMinutes * 60;

        return (tokenString, expiresInSeconds);
    }
}
