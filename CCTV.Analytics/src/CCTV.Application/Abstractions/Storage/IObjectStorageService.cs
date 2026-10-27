namespace CCTV.Application.Abstractions.Storage;

public interface IObjectStorageService
{
    /// <summary>
    /// Mengunggah file stream gambar atau video bukti ke bucket object storage.
    /// </summary>
    /// <param name="data">Stream data file</param>
    /// <param name="objectKey">Path atau key unik objek di dalam bucket</param>
    /// <param name="contentType">MIME type file, contoh: image/jpeg, video/mp4</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>URI atau URL referensi penyimpanan objek</returns>
    Task<string> UploadAsync(Stream data, string objectKey, string contentType = "image/jpeg", CancellationToken cancellationToken = default);

    /// <summary>
    /// Mengunduh stream objek dari bucket object storage berdasarkan objectKey.
    /// </summary>
    /// <param name="objectKey">Key objek</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Stream data</returns>
    Task<Stream?> DownloadAsync(string objectKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Menghasilkan URL bertandatangan (presigned URL) untuk akses sementara yang aman tanpa membuka bucket ke publik.
    /// </summary>
    /// <param name="objectKey">Key objek</param>
    /// <param name="expiry">Masa berlaku URL</param>
    /// <returns>Presigned URL string</returns>
    Task<string> GetPresignedUrlAsync(string objectKey, TimeSpan expiry);

    /// <summary>
    /// Menghapus objek bukti dari storage.
    /// </summary>
    /// <param name="objectKey">Key objek yang akan dihapus</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> DeleteAsync(string objectKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mengecek apakah objek ada di storage.
    /// </summary>
    Task<bool> ExistsAsync(string objectKey, CancellationToken cancellationToken = default);
}
