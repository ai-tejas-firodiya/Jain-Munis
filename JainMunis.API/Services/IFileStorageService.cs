namespace JainMunis.API.Services;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string folder, string? fileName = null);
    Task<bool> DeleteFileAsync(string fileUrl);
    Task<bool> FileExistsAsync(string fileUrl);
    Task<Stream?> DownloadFileAsync(string fileUrl);
    Task<string> GeneratePresignedUrlAsync(string fileUrl, TimeSpan expiration);
}

public enum FileStorageProvider
{
    Local,
    AzureBlobStorage,
    AWSS3,
    GoogleCloudStorage
}

public class FileUploadResult
{
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Hash { get; set; } = string.Empty;
}

public class FileValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public long MaxFileSize { get; set; }
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
}