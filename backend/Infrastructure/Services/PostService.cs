using UrbaPF.Infrastructure.Interfaces;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Domain.Enums;

namespace UrbaPF.Infrastructure.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _repo;

    public PostService(IPostRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<PostDto>> GetByCondominiumAsync(Guid condominiumId)
    {
        return await _repo.GetByCondominiumAsync(condominiumId);
    }

    public async Task<PostDto?> GetByIdAsync(Guid id)
    {
        return await _repo.GetByIdAsync(id);
    }

    public async Task<Guid> CreateAsync(Guid condominiumId, Guid userId, CreatePostDto request)
    {
        var status = (int)PostStatus.PendingApproval;
        return await _repo.CreateAsync(new CreatePostDto
        {
            Title = request.Title,
            Content = request.Content,
            IsPinned = request.IsPinned,
            IsAnnouncement = request.IsAnnouncement,
            Category = request.Category,
            Status = status
        }, userId, condominiumId, status);
    }

    public async Task<bool> UpdateAsync(Guid id, Guid userId, UserRole userRole, UpdatePostDto request)
    {
        var post = await _repo.GetByIdAsync(id);
        if (post == null) return false;

        var isAuthor = post.AuthorId == userId;
        var isAdminOrManager = userRole == UserRole.Administrator || userRole == UserRole.Manager;

        if (!isAuthor && !isAdminOrManager) return false;

        var updateDto = new UpdatePostDto
        {
            Title = request.Title,
            Content = request.Content,
            IsPinned = request.IsPinned,
            IsAnnouncement = request.IsAnnouncement,
            Category = request.Category,
            Status = request.Status
        };

        await _repo.UpdateAsync(id, updateDto);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, UserRole userRole)
    {
        var post = await _repo.GetByIdAsync(id);
        if (post == null) return false;

        var isAuthor = post.AuthorId == userId;
        var isAdmin = userRole == UserRole.Administrator;

        if (!isAuthor && !isAdmin) return false;

        await _repo.SoftDeleteAsync(id);
        return true;
    }
}