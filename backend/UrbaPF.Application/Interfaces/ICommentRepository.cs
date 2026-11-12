using UrbaPF.Application.DTOs;

namespace UrbaPF.Application.Interfaces;

public interface ICommentRepository
{
    Task<IEnumerable<CommentDto>> GetByPostAsync(Guid postId);
    Task<CommentDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(CreateCommentDto dto, Guid userId, Guid postId);
    Task SoftDeleteAsync(Guid id);
}
