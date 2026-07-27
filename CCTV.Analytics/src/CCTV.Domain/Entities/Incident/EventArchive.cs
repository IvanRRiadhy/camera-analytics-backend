using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Incident;

public class EventArchive : TenantEntity
{
    public Guid IncidentId { get; set; }
    public Incident Incident { get; set; } = null!;

    public string ArchiveBatchCode { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int RecordCount { get; set; }
    public string StorageLocationUrl { get; set; } = string.Empty;
    public string Sha256Checksum { get; set; } = string.Empty;
}
