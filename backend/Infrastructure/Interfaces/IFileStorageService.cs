namespace UrbaPF.Infrastructure.Interfaces;

public interface IFileStorageService
{
    // Este método recibirá un stream de bytes y el nombre del archivo
    // y devolverá la URL donde se almacenó.
    Task<(string? Url, string? Error)> SaveFileAsync(Stream fileStream, string fileName, string contentType);
}