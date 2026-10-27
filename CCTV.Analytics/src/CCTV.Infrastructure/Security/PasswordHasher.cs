using CCTV.Application.Identity.Interfaces;

namespace CCTV.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password) => BCrypt.Net.BCrypt.EnhancedHashPassword(password);

    public bool VerifyPassword(string password, string passwordHash) => BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash);
}
