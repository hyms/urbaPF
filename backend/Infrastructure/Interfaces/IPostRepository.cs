using UrbaPF.Infrastructure.DTOs;

namespace UrbaPF.Infrastructure.Interfaces;

public interface IPostRepository
{
    Task<IEnumerable<PostDto>> GetAllAsync();
    Task<PostDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<PostDto>> GetByCondominiumAsync(Guid condominiumId);
    Task<Guid> CreateAsync(CreatePostDto dto, Guid userId, Guid condominiumId);
    Task UpdateAsync(Guid id, UpdatePostDto dto);
    Task SoftDeleteAsync(Guid id);
    Task IncrementViewCountAsync(Guid id);
}
