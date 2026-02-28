using UrbaPF.Infrastructure.DTOs;

namespace UrbaPF.Infrastructure.Interfaces;

public interface ICommentRepository
{
    Task<IEnumerable<CommentDto>> GetByPostAsync(Guid postId);
    Task<CommentDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(CreateCommentDto dto, Guid userId, Guid postId);
    Task SoftDeleteAsync(Guid id);
}
