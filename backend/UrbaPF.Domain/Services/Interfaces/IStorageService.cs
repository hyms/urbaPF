namespace UrbaPF.Domain.Services.Interfaces;

public interface IStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string container);
    Task<Stream> GetFileAsync(string filePath);
    Task<bool> DeleteFileAsync(string filePath);
    string GetFileUrl(string filePath);
    string GetLocalPath(string filePath);
}
