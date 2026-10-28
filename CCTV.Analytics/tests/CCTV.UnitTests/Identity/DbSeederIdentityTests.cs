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

    [Fact]
    public void SeededDefaultOperatorPassword_VerifiesCorrectly()
    {
        var password = "Operator@123456";
        var hash = _hasher.HashPassword(password);

        Assert.True(_hasher.VerifyPassword(password, hash));
        Assert.False(_hasher.VerifyPassword("WrongOperatorPassword", hash));
    }

    [Fact]
    public void SeededRolePermissions_AdminHasAll_OperatorHasSubset()
    {
        Assert.Equal(11, CCTV.Domain.Entities.Identity.AppPermissions.All.Count);
        Assert.Equal(6, CCTV.Domain.Entities.Identity.AppPermissions.OperatorDefaults.Count);
        Assert.Contains(CCTV.Domain.Entities.Identity.AppPermissions.Spatial.Write, CCTV.Domain.Entities.Identity.AppPermissions.All);
        Assert.DoesNotContain(CCTV.Domain.Entities.Identity.AppPermissions.Spatial.Write, CCTV.Domain.Entities.Identity.AppPermissions.OperatorDefaults);
    }
}
