namespace CCTV.Shared.Exceptions;

public class TenantNotResolvedException : Exception
{
    public TenantNotResolvedException() 
        : base("Tenant identity could not be resolved from the request context.") { }

    public TenantNotResolvedException(string message) 
        : base(message) { }

    public TenantNotResolvedException(string message, Exception innerException) 
        : base(message, innerException) { }
}
