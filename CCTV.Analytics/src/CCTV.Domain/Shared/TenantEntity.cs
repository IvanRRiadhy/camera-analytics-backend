namespace CCTV.Domain.Shared;

public abstract class TenantEntity : BaseEntity
{
    public Guid ApplicationId { get; set; }
}
