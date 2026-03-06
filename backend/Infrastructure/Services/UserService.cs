using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces; // Added for IUserRepository and IFileStorageService

namespace UrbaPF.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IFileStorageService _fileStorageService;

    public UserService(IUserRepository userRepository, IFileStorageService fileStorageService)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
    }

    public async Task<Guid?> UploadUserPhotoAsync(Guid userId, IFormFile file, ClaimsPrincipal user)
    {
        // Basic validation
        if (file == null || file.Length == 0)
        {
            // Consider throwing an exception or returning a specific error
            return null; 
        }

        // Upload the file
        var photoUrl = await _fileStorageService.UploadFileAsync(file, $"user-photos/{userId}/{Guid.NewGuid()}");

        if (photoUrl == null)
        {
            // Handle upload failure
            return null;
        }

        // Update the user's photo URL in the repository
        await _userRepository.UpdateUserPhotoUrlAsync(userId, photoUrl);

        return userId; // Indicate success by returning the user ID
    }
}