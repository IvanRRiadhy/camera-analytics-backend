using CCTV.Contracts.Common;

namespace CCTV.Contracts.Analytics;

public record DetectionEvidenceMessage : BaseMessage
{
    public Guid EvidenceId { get; init; }
    public Guid EventId { get; init; }
    public string StorageProvider { get; init; } = "LocalStorage";
    public string BucketName { get; init; } = "evidence";
    public string ObjectKey { get; init; } = string.Empty;
    public string? CropImageKey { get; init; }
    public string? FullFrameKey { get; init; }
    public string? VideoSnippetKey { get; init; }
    public long FileSizeBytes { get; init; }
    public string MimeType { get; init; } = "image/jpeg";
    public string Sha256Checksum { get; init; } = string.Empty;
}
