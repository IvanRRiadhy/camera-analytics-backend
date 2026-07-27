namespace CCTV.Shared.Interfaces;

public interface ITenantProvider
{
    Guid ApplicationId { get; }
    bool IsTenantSet { get; }
}
