using System.Net.Http.Headers;
using CCTV.Application.Abstractions.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CCTV.Infrastructure.Storage;

/// <summary>
/// Implementasi klien S3-compatible Object Storage yang mendukung SeaweedFS, MinIO, dan AWS S3.
/// Menggunakan REST HTTP client langsung untuk kompatibilitas native berkecepatan tinggi tanpa beban dependensi eksternal.
/// </summary>
public class S3CompatibleStorageService : IObjectStorageService
{
    private readonly HttpClient _httpClient;
    private readonly StorageOptions _options;
    private readonly ILogger<S3CompatibleStorageService> _logger;

    public S3CompatibleStorageService(
        HttpClient httpClient,
        IOptions<StorageOptions> options,
        ILogger<S3CompatibleStorageService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        if (!string.IsNullOrEmpty(_options.ServiceUrl))
        {
            _httpClient.BaseAddress = new Uri(_options.ServiceUrl.TrimEnd('/') + "/");
        }
    }

    public async Task<string> UploadAsync(
        Stream data,
        string objectKey,
        string contentType = "image/jpeg",
        CancellationToken cancellationToken = default)
    {
        var targetPath = $"{_options.BucketName}/{objectKey.TrimStart('/')}";
        _logger.LogInformation("Uploading visual evidence to S3/SeaweedFS: {Path}", targetPath);

        using var content = new StreamContent(data);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        var response = await _httpClient.PutAsync(targetPath, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var publicUrl = $"{_options.PublicBaseUrl.TrimEnd('/')}/{targetPath}";
        _logger.LogInformation("Visual evidence uploaded successfully. URL: {Url}", publicUrl);
        return publicUrl;
    }

    public async Task<Stream?> DownloadAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        var targetPath = $"{_options.BucketName}/{objectKey.TrimStart('/')}";
        var response = await _httpClient.GetAsync(targetPath, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to download object {Path}. Status: {StatusCode}", targetPath, response.StatusCode);
            return null;
        }

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public Task<string> GetPresignedUrlAsync(string objectKey, TimeSpan expiry)
    {
        // Untuk SeaweedFS / S3 endpoint lokal, URL dapat diakses langsung via Filer / Public URL
        var targetPath = $"{_options.BucketName}/{objectKey.TrimStart('/')}";
        var presigned = $"{_options.PublicBaseUrl.TrimEnd('/')}/{targetPath}";
        return Task.FromResult(presigned);
    }

    public async Task<bool> DeleteAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        var targetPath = $"{_options.BucketName}/{objectKey.TrimStart('/')}";
        var response = await _httpClient.DeleteAsync(targetPath, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ExistsAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        var targetPath = $"{_options.BucketName}/{objectKey.TrimStart('/')}";
        using var request = new HttpRequestMessage(HttpMethod.Head, targetPath);
        var response = await _httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
