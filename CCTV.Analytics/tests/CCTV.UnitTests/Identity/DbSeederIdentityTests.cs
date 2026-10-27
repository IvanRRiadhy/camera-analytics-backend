using CCTV.Infrastructure.Security;
using Xunit;

namespace CCTV.UnitTests.Identity;

public class DbSeederIdentityTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void SeededDefaultAdminPassword_VerifiesCorrectly()
    {
        var password = "Admin@123456";
        var hash = _hasher.HashPassword(password);

        Assert.True(_hasher.VerifyPassword(password, hash));
        Assert.False(_hasher.VerifyPassword("WrongAdminPassword", hash));
    }
}
