namespace UrbaPF.Infrastructure.Interfaces;

public interface IAuthService
{
    Task<(string? Token, string? Error)> LoginAsync(string email, string password);
    Task<(Guid? UserId, string? Error)> RegisterAsync(string email, string password, string fullName, string? phone);
    Task<(bool Success, string? Error)> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);
}
