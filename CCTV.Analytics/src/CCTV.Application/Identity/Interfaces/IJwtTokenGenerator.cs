using CCTV.Domain.Entities.Identity;

namespace CCTV.Application.Identity.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, int ExpiresInSeconds) GenerateToken(User user, IList<string> roles);
}
