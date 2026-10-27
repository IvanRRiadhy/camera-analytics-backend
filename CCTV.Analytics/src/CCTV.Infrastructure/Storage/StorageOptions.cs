namespace CCTV.Infrastructure.Storage;

public class StorageOptions
{
    public const string SectionName = "Storage";

    public string Provider { get; set; } = "SeaweedFS";
    public string ServiceUrl { get; set; } = "http://localhost:8333";
    public string BucketName { get; set; } = "cctv-evidence";
    public string AccessKey { get; set; } = "cctv_storage_admin";
    public string SecretKey { get; set; } = "SecureCctvStoragePass123";
    public string PublicBaseUrl { get; set; } = "http://localhost:8888";
    public bool ForcePathStyle { get; set; } = true;
}
