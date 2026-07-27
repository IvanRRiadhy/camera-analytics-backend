using CCTV.Shared.Exceptions;
using CCTV.Shared.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace CCTV.Api.Providers;

public class HttpContextTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHostEnvironment _environment;
    private static readonly Guid DefaultDevApplicationId = Guid.Parse("e078ee93-90c4-4b95-9651-7bf82c8a6d33");

    public HttpContextTenantProvider(IHttpContextAccessor httpContextAccessor, IHostEnvironment environment)
    {
        _httpContextAccessor = httpContextAccessor;
        _environment = environment;
    }

    public Guid ApplicationId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext != null)
            {
                // 1. Extract from Verified JWT Claims (app_id or tenant_id)
                var tenantClaim = httpContext.User?.FindFirst("app_id")?.Value
                                  ?? httpContext.User?.FindFirst("tenant_id")?.Value;

                if (!string.IsNullOrEmpty(tenantClaim) && Guid.TryParse(tenantClaim, out var tenantId))
                {
                    return tenantId;
                }

                // 2. Extract from Request Header for Inter-Service Calls
                if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var headerTenant) &&
                    Guid.TryParse(headerTenant, out var headerTenantId))
                {
                    return headerTenantId;
                }
            }

            // In Development environment ONLY, allow fallback GUID for rapid local testing
            if (_environment.IsDevelopment())
            {
                return DefaultDevApplicationId;
            }

            // In Production, throw TenantNotResolvedException immediately
            throw new TenantNotResolvedException("Tenant identity could not be resolved from JWT claims or HTTP request headers.");
        }
    }

    public bool IsTenantSet => ApplicationId != Guid.Empty;
}
