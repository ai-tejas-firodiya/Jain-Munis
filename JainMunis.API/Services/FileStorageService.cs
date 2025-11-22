using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace JainMunis.API.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileStorageService> _logger;
    private readonly string _connectionString;
    private readonly string _containerName;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _containerClient;

    public FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var connectionString = _configuration.GetSection("FileStorage:AzureBlobStorage:ConnectionString").Value;
        _containerName = _configuration.GetSection("FileStorage:AzureBlobStorage:ContainerName").Value ?? "saint-photos";

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Azure Blob Storage connection string is not configured.");
        }

        _connectionString = connectionString;
        _blobServiceClient = new BlobServiceClient(_connectionString);
        _containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

        // Create container if it doesn't exist
        _containerClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folder, string? fileName = null)
    {
        try
        {
            // Validate file
            var validationResult = ValidateFile(file);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException(validationResult.ErrorMessage);
            }

            // Generate unique filename
            fileName = fileName ?? GenerateUniqueFileName(file.FileName);
            var blobName = $"{folder}/{fileName}";

            // Get blob client
            var blobClient = _containerClient.GetBlobClient(blobName);

            // Configure blob options
            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = file.ContentType,
                CacheControl = "public, max-age=31536000" // 1 year cache
            };

            // Set metadata
            var metadata = new Dictionary<string, string>
            {
                ["OriginalName"] = file.FileName,
                ["UploadDate"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                ["FileSize"] = file.Length.ToString(),
                ["ContentType"] = file.ContentType,
                ["MD5Hash"] = await CalculateMD5Hash(file)
            };

            // Upload file with simplified options
            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeaders,
                Metadata = metadata
            });

            var fileUrl = blobClient.Uri.ToString();
            _logger.LogInformation("File uploaded successfully: {FileName} to {Url}", fileName, fileUrl);

            return fileUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", fileName ?? file.FileName);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            var blobName = ExtractBlobNameFromUrl(fileUrl);
            if (string.IsNullOrEmpty(blobName))
            {
                _logger.LogWarning("Invalid file URL for deletion: {Url}", fileUrl);
                return false;
            }

            var blobClient = _containerClient.GetBlobClient(blobName);
            var response = await blobClient.DeleteIfExistsAsync();

            if (response.Value)
            {
                _logger.LogInformation("File deleted successfully: {Url}", fileUrl);
            }
            else
            {
                _logger.LogWarning("File not found for deletion: {Url}", fileUrl);
            }

            return response.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {Url}", fileUrl);
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(string fileUrl)
    {
        try
        {
            var blobName = ExtractBlobNameFromUrl(fileUrl);
            if (string.IsNullOrEmpty(blobName))
            {
                return false;
            }

            var blobClient = _containerClient.GetBlobClient(blobName);
            return await blobClient.ExistsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence: {Url}", fileUrl);
            return false;
        }
    }

    public async Task<Stream?> DownloadFileAsync(string fileUrl)
    {
        try
        {
            var blobName = ExtractBlobNameFromUrl(fileUrl);
            if (string.IsNullOrEmpty(blobName))
            {
                return null;
            }

            var blobClient = _containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync())
            {
                return null;
            }

            var response = await blobClient.DownloadStreamingAsync();
            return response.Value.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file: {Url}", fileUrl);
            return null;
        }
    }

    public async Task<string> GeneratePresignedUrlAsync(string fileUrl, TimeSpan expiration)
    {
        try
        {
            var blobName = ExtractBlobNameFromUrl(fileUrl);
            if (string.IsNullOrEmpty(blobName))
            {
                return string.Empty;
            }

            var blobClient = _containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync())
            {
                return string.Empty;
            }

            // Note: Azure Blob Storage doesn't have built-in presigned URL generation like AWS S3
            // You would need to implement a custom token-based system or use Shared Access Signatures
            // For now, returning the regular URL
            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL for: {Url}", fileUrl);
            return string.Empty;
        }
    }

    private FileValidationResult ValidateFile(IFormFile file)
    {
        var maxFileSize = _configuration.GetValue<long>("FileStorage:MaxFileSize", 5242880); // 5MB default
        var allowedExtensions = _configuration.GetSection("FileStorage:AllowedExtensions").Get<string[]>()
            ?? new[] { ".jpg", ".jpeg", ".png", ".webp" };

        var result = new FileValidationResult
        {
            IsValid = true,
            MaxFileSize = maxFileSize,
            AllowedExtensions = allowedExtensions
        };

        // Check file size
        if (file.Length > maxFileSize)
        {
            result.IsValid = false;
            result.ErrorMessage = $"File size exceeds the maximum allowed size of {maxFileSize / (1024 * 1024)}MB";
            return result;
        }

        // Check file extension
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            result.IsValid = false;
            result.ErrorMessage = $"File type '{fileExtension}' is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}";
            return result;
        }

        // Validate content type
        var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
        if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            result.IsValid = false;
            result.ErrorMessage = $"Content type '{file.ContentType}' is not allowed";
            return result;
        }

        return result;
    }

    private string GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var guid = Guid.NewGuid().ToString("N")[..8];

        // Sanitize filename
        fileNameWithoutExtension = Regex.Replace(fileNameWithoutExtension, @"[^a-zA-Z0-9\-_]", "");

        return $"{fileNameWithoutExtension}_{timestamp}_{guid}{extension}";
    }

    private async Task<string> CalculateMD5Hash(IFormFile file)
    {
        using var md5 = MD5.Create();
        using var stream = file.OpenReadStream();
        var hashBytes = await md5.ComputeHashAsync(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    private string? ExtractBlobNameFromUrl(string fileUrl)
    {
        try
        {
            var uri = new Uri(fileUrl);
            var containerPath = _containerClient.Uri.AbsoluteUri;

            if (!fileUrl.StartsWith(containerPath))
            {
                return null;
            }

            return fileUrl.Substring(containerPath.Length).TrimStart('/');
        }
        catch
        {
            return null;
        }
    }
}