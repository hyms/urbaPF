using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UrbaPF.Infrastructure.Interfaces;

public interface IFileStorageService
{
    Task<string?> UploadFileAsync(IFormFile file, string path);
}