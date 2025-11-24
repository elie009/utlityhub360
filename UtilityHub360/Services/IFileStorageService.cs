namespace UtilityHub360.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(Stream fileStream, string fileName, string userId, string fileType);
        Task<Stream> GetFileAsync(string filePath);
        Task<bool> DeleteFileAsync(string filePath);
        Task<string> SaveThumbnailAsync(Stream imageStream, string originalFileName, string userId);
        string GetFileUrl(string filePath);
    }
}

