using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace JainMunis.API.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly string _storagePath;

    public LocalFileStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        _storagePath = _configuration.GetSection("FileStorage:LocalStoragePath").Value ?? "./uploads";

        // Ensure storage directory exists
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
            _logger.LogInformation("Created local storage directory: {Path}", _storagePath);
        }
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
            var folderPath = Path.Combine(_storagePath, folder);
            var filePath = Path.Combine(folderPath, fileName);

            // Ensure folder exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Save file
            using var fileStream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fileStream);

            var fileUrl = $"/uploads/{folder}/{fileName}";
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
        await Task.CompletedTask;
        try
        {
            var relativePath = ExtractRelativePathFromUrl(fileUrl);
            if (string.IsNullOrEmpty(relativePath))
            {
                _logger.LogWarning("Invalid file URL for deletion: {Url}", fileUrl);
                return false;
            }

            var filePath = Path.Combine(_storagePath, relativePath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted successfully: {Url}", fileUrl);
                return true;
            }
            else
            {
                _logger.LogWarning("File not found for deletion: {Url}", fileUrl);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {Url}", fileUrl);
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(string fileUrl)
    {
        await Task.CompletedTask;
        try
        {
            var relativePath = ExtractRelativePathFromUrl(fileUrl);
            if (string.IsNullOrEmpty(relativePath))
            {
                return false;
            }

            var filePath = Path.Combine(_storagePath, relativePath);
            return File.Exists(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence: {Url}", fileUrl);
            return false;
        }
    }

    public async Task<Stream?> DownloadFileAsync(string fileUrl)
    {
        await Task.CompletedTask;
        try
        {
            var relativePath = ExtractRelativePathFromUrl(fileUrl);
            if (string.IsNullOrEmpty(relativePath))
            {
                return null;
            }

            var filePath = Path.Combine(_storagePath, relativePath);

            if (!File.Exists(filePath))
            {
                return null;
            }

            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file: {Url}", fileUrl);
            return null;
        }
    }

    public async Task<string> GeneratePresignedUrlAsync(string fileUrl, TimeSpan expiration)
    {
        await Task.CompletedTask;
        // Local storage doesn't support presigned URLs like cloud providers
        // Return the regular URL - in a real implementation, you might implement
        // a temporary token-based system
        return fileUrl;
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

    private string? ExtractRelativePathFromUrl(string fileUrl)
    {
        try
        {
            if (fileUrl.StartsWith("/uploads/"))
            {
                return fileUrl.Substring("/uploads/".Length);
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
}