using UrbaPF.Application.DTOs;
using UrbaPF.Domain.Enums;

namespace UrbaPF.Application.Interfaces;

public interface IPostService
{
    Task<IEnumerable<PostDto>> GetByCondominiumAsync(Guid condominiumId);
    Task<PostDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(Guid condominiumId, Guid userId, CreatePostDto request);
    Task<bool> UpdateAsync(Guid id, Guid userId, UserRole userRole, UpdatePostDto request);
    Task<bool> DeleteAsync(Guid id, Guid userId, UserRole userRole);
}
