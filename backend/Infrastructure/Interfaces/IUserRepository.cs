using UrbaPF.Infrastructure.DTOs;

namespace UrbaPF.Infrastructure.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<UserDto?> GetByEmailAsync(string email);
    Task<UserDto?> GetByEmailWithPasswordAsync(string email);
    Task<Guid> CreateAsync(CreateUserDto dto, string passwordHash);
    Task UpdateAsync(Guid id, UpdateUserDto dto);
    Task SoftDeleteAsync(Guid id);
    Task UpdateLastLoginAsync(Guid id);
    Task UpdatePasswordHashAsync(Guid id, string passwordHash);
}
