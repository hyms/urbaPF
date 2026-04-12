using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Domain.Enums;

namespace UrbaPF.Infrastructure.Interfaces;

public interface IAuthService
{
    Task<(AuthResponseDto? Response, string? Error)> LoginAsync(string email, string password);
    Task<(Guid? UserId, string? Error)> RegisterUserAsync(CreateUserDto createUserDto, string password, UserRole role);
    Task<(bool Success, string? Error)> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);
    Task<(AuthResponseDto? Response, string? Error)> RefreshTokenAsync(string refreshToken);
    Task<(bool Success, string? Error)> RevokeRefreshTokenAsync(string refreshToken);
}
