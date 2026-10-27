using CCTV.Domain.Enumerations;
using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Incident;

public class DetectionEvidence : TenantEntity
{
    public Guid EventId { get; set; }
    public AnalyticsEvent Event { get; set; } = null!;

    public StorageProvider StorageProvider { get; set; } = StorageProvider.LocalStorage;
    public string BucketName { get; set; } = "evidence";
    public string ObjectKey { get; set; } = string.Empty;
    public string? CropImageObjectKey { get; set; }
    public string? FullFrameObjectKey { get; set; }
    public string? VideoSnippetObjectKey { get; set; }

    /// <summary>
    /// Dual Evidence: URL presigned atau lokasi penyimpanan frame asli tanpa modifikasi.
    /// </summary>
    public string? OriginalMediaUrl { get; set; }

    /// <summary>
    /// Dual Evidence: URL presigned atau lokasi crop ROI yang ditingkatkan (CLAHE, denoise, tanpa halusinasi).
    /// </summary>
    public string? EnhancedMediaUrl { get; set; }

    public long FileSizeBytes { get; set; }
    public string MimeType { get; set; } = "image/jpeg";
    public string EncryptionAlgorithm { get; set; } = "AES_256";
    public string Sha256Checksum { get; set; } = string.Empty;
    public int RetentionPolicyDays { get; set; } = 30;
    public DateTime RetentionExpiryDate { get; set; } = DateTime.UtcNow.AddDays(30);
    public bool IsLockedForLegalHold { get; set; } = false;
}
