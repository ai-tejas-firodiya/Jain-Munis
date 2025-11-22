using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace JainMunis.API.Services;

public class AWSS3FileStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AWSS3FileStorageService> _logger;
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public AWSS3FileStorageService(IConfiguration configuration, ILogger<AWSS3FileStorageService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var accessKey = _configuration.GetSection("FileStorage:AWSS3:AccessKey").Value;
        var secretKey = _configuration.GetSection("FileStorage:AWSS3:SecretKey").Value;
        var region = _configuration.GetSection("FileStorage:AWSS3:Region").Value ?? "ap-south-1";
        _bucketName = _configuration.GetSection("FileStorage:AWSS3:BucketName").Value ?? "";

        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(_bucketName))
        {
            throw new InvalidOperationException("AWS S3 configuration is incomplete. Please provide AccessKey, SecretKey, and BucketName.");
        }

        var config = new AmazonS3Config
        {
            RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region)
        };

        _s3Client = new AmazonS3Client(accessKey, secretKey, config);
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
            var key = $"{folder}/{fileName}";

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = memoryStream,
                ContentType = file.ContentType,
                CannedACL = S3CannedACL.PublicRead
            };

            // Add metadata as object metadata instead of the read-only property
            request.Metadata["OriginalName"] = file.FileName;
            request.Metadata["UploadDate"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            request.Metadata["FileSize"] = file.Length.ToString();

            // Add tagging via TagSet
            request.TagSet = new List<Tag>
            {
                new Tag { Key = "Environment", Value = "Production" },
                new Tag { Key = "App", Value = "JainMunis" }
            };

            var response = await _s3Client.PutObjectAsync(request);
            var fileUrl = $"https://{_bucketName}.s3.amazonaws.com/{key}";

            _logger.LogInformation("File uploaded successfully to S3: {FileName} to {Url}", fileName, fileUrl);
            return fileUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to S3: {FileName}", fileName ?? file.FileName);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            var key = ExtractKeyFromUrl(fileUrl);
            if (string.IsNullOrEmpty(key))
            {
                _logger.LogWarning("Invalid S3 URL for deletion: {Url}", fileUrl);
                return false;
            }

            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            var response = await _s3Client.DeleteObjectAsync(request);

            _logger.LogInformation("File deleted from S3: {Url}", fileUrl);
            return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from S3: {Url}", fileUrl);
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(string fileUrl)
    {
        try
        {
            var key = ExtractKeyFromUrl(fileUrl);
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            var request = new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            try
            {
                await _s3Client.GetObjectMetadataAsync(request);
                return true;
            }
            catch (Amazon.S3.AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence in S3: {Url}", fileUrl);
            return false;
        }
    }

    public async Task<Stream?> DownloadFileAsync(string fileUrl)
    {
        try
        {
            var key = ExtractKeyFromUrl(fileUrl);
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectAsync(request);
            return response.ResponseStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from S3: {Url}", fileUrl);
            return null;
        }
    }

    public async Task<string> GeneratePresignedUrlAsync(string fileUrl, TimeSpan expiration)
    {
        await Task.CompletedTask;
        try
        {
            var key = ExtractKeyFromUrl(fileUrl);
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Expires = DateTime.UtcNow.Add(expiration)
            };

            var url = _s3Client.GetPreSignedURL(request);
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL for S3: {Url}", fileUrl);
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

    private string? ExtractKeyFromUrl(string fileUrl)
    {
        try
        {
            var uri = new Uri(fileUrl);
            var bucketPattern = $@"^{_bucketName}\.s3\.amazonaws\.com";
            var match = Regex.Match(fileUrl, bucketPattern);

            if (!match.Success)
            {
                return null;
            }

            var keyStart = match.Length;
            if (fileUrl.Length <= keyStart)
            {
                return null;
            }

            return fileUrl.Substring(keyStart).TrimStart('/');
        }
        catch
        {
            return null;
        }
    }
}