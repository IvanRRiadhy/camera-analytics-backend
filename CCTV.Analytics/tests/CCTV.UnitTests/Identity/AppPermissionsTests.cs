using CCTV.Domain.Entities.Identity;

namespace CCTV.UnitTests.Identity;

public class AppPermissionsTests
{
    [Fact]
    public void AllPermissions_ShouldHaveNoDuplicates()
    {
        var distinctCount = AppPermissions.All.Distinct(StringComparer.OrdinalIgnoreCase).Count();
        Assert.Equal(AppPermissions.All.Count, distinctCount);
    }

    [Fact]
    public void OperatorDefaults_ShouldBeSubsetOfAll()
    {
        Assert.All(AppPermissions.OperatorDefaults, perm =>
            Assert.Contains(perm, AppPermissions.All));
    }

    [Theory]
    [InlineData("spatial.read")]
    [InlineData("spatial.write")]
    [InlineData("analytics.read")]
    [InlineData("analytics.write")]
    [InlineData("orchestrator.read")]
    [InlineData("orchestrator.write")]
    [InlineData("orchestrator.execute")]
    [InlineData("monitoring.read")]
    [InlineData("monitoring.write")]
    [InlineData("users.read")]
    [InlineData("users.write")]
    public void AllPermissions_ShouldContainStandardPermissions(string expected)
    {
        Assert.Contains(expected, AppPermissions.All);
    }

    [Fact]
    public void OperatorDefaults_ShouldContainMonitoringAndEmergencyExecute()
    {
        Assert.Contains(AppPermissions.Monitoring.Read, AppPermissions.OperatorDefaults);
        Assert.Contains(AppPermissions.Monitoring.Write, AppPermissions.OperatorDefaults);
        Assert.Contains(AppPermissions.Orchestrator.Execute, AppPermissions.OperatorDefaults);
        Assert.DoesNotContain(AppPermissions.Users.Write, AppPermissions.OperatorDefaults);
    }
}
