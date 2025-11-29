using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace UtilityHub360.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _basePath;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(IWebHostEnvironment environment, ILogger<FileStorageService> logger)
        {
            _environment = environment;
            _logger = logger;
            _basePath = Path.Combine(_environment.ContentRootPath, "uploads", "receipts");
            
            // Ensure directory exists
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string userId, string fileType)
        {
            try
            {
                // Create user-specific directory
                var userDir = Path.Combine(_basePath, userId);
                if (!Directory.Exists(userDir))
                {
                    Directory.CreateDirectory(userDir);
                }

                // Generate unique filename
                var fileExtension = Path.GetExtension(fileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(userDir, uniqueFileName);

                // Save file
                using (var fileStreamWriter = new FileStream(filePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(fileStreamWriter);
                }

                // Return relative path
                return Path.Combine("receipts", userId, uniqueFileName).Replace("\\", "/");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file {FileName} for user {UserId}", fileName, userId);
                throw;
            }
        }

        public async Task<Stream> GetFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.ContentRootPath, "uploads", filePath);
                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file {FilePath}", filePath);
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.ContentRootPath, "uploads", filePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FilePath}", filePath);
                return false;
            }
        }

        public async Task<string> SaveThumbnailAsync(Stream imageStream, string originalFileName, string userId)
        {
            try
            {
                var userDir = Path.Combine(_basePath, userId, "thumbnails");
                if (!Directory.Exists(userDir))
                {
                    Directory.CreateDirectory(userDir);
                }

                var fileExtension = Path.GetExtension(originalFileName);
                var uniqueFileName = $"thumb_{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(userDir, uniqueFileName);

                using (var fileStreamWriter = new FileStream(filePath, FileMode.Create))
                {
                    await imageStream.CopyToAsync(fileStreamWriter);
                }

                return Path.Combine("receipts", userId, "thumbnails", uniqueFileName).Replace("\\", "/");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving thumbnail for user {UserId}", userId);
                throw;
            }
        }

        public string GetFileUrl(string filePath)
        {
            // Return relative URL that can be served by the API
            return $"/api/receipts/files/{filePath}";
        }
    }
}

