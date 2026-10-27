namespace CCTV.Domain.Enumerations;

public enum StorageProvider
{
    LocalStorage = 0,
    AzureBlobStorage = 1,
    AWS_S3 = 2,
    MinIO = 3,
    SeaweedFS = 4
}
