using System.Security.Claims;
using CCTV.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace CCTV.UnitTests.Identity;

public class PermissionPolicyProviderTests
{
    private readonly PermissionPolicyProvider _provider;

    public PermissionPolicyProviderTests()
    {
        var options = Options.Create(new AuthorizationOptions());
        _provider = new PermissionPolicyProvider(options);
    }

    [Fact]
    public async Task GetPolicyAsync_WithPermissionPrefix_BuildsPolicyWithAdminAndWildcardSupport()
    {
        var policy = await _provider.GetPolicyAsync("perm:spatial.read");

        Assert.NotNull(policy);
        var requirement = Assert.Single(policy.Requirements.OfType<Microsoft.AspNetCore.Authorization.Infrastructure.AssertionRequirement>());
        Assert.NotNull(requirement);

        // 1. Admin role bypasses check
        var adminPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }, "TestAuth"));
        var adminContext = new AuthorizationHandlerContext(policy.Requirements, adminPrincipal, null);
        Assert.True(await requirement.Handler(adminContext));

        // 2. Wildcard * claim bypasses check
        var wildcardPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("permission", "*") }, "TestAuth"));
        var wildcardContext = new AuthorizationHandlerContext(policy.Requirements, wildcardPrincipal, null);
        Assert.True(await requirement.Handler(wildcardContext));

        // 3. Matching permission succeeds
        var matchingPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("permission", "spatial.read") }, "TestAuth"));
        var matchingContext = new AuthorizationHandlerContext(policy.Requirements, matchingPrincipal, null);
        Assert.True(await requirement.Handler(matchingContext));

        // 4. Non-matching permission without wildcard fails
        var nonMatchingPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("permission", "spatial.write") }, "TestAuth"));
        var nonMatchingContext = new AuthorizationHandlerContext(policy.Requirements, nonMatchingPrincipal, null);
        Assert.False(await requirement.Handler(nonMatchingContext));
    }

    [Fact]
    public async Task GetPolicyAsync_WithStandardPolicy_FallsBackToDefaultProvider()
    {
        var policy = await _provider.GetPolicyAsync("NonExistentStandardPolicy");
        Assert.Null(policy);
    }
}
