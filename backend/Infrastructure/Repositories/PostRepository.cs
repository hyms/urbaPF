using Dapper;
using UrbaPF.Infrastructure.Data;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;

namespace UrbaPF.Infrastructure.Repositories;

public class PostRepository : BaseRepository, IPostRepository
{
    public PostRepository(DbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<IEnumerable<PostDto>> GetAllAsync()
    {
        var sql = @"
            SELECT p.id, p.condominium_id as CondominiumId, p.author_id as AuthorId, p.title, p.content, 
                   p.category, p.is_pinned as IsPinned, p.is_announcement as IsAnnouncement, 
                   p.status, p.view_count as ViewCount, p.created_at as CreatedAt, p.updated_at as UpdatedAt,
                   u.full_name as AuthorName
            FROM posts p
            JOIN users u ON p.author_id = u.id
            WHERE p.deleted_at IS NULL
            ORDER BY p.is_pinned DESC, p.created_at DESC";
        return await QueryAsync<PostDto>(sql);
    }

    public async Task<IEnumerable<PostDto>> GetByCondominiumAsync(Guid condominiumId)
    {
        var sql = @"
            SELECT p.id, p.condominium_id as CondominiumId, p.author_id as AuthorId, p.title, p.content, 
                   p.category, p.is_pinned as IsPinned, p.is_announcement as IsAnnouncement, 
                   p.status, p.view_count as ViewCount, p.created_at as CreatedAt, p.updated_at as UpdatedAt,
                   u.full_name as AuthorName
            FROM posts p
            JOIN users u ON p.author_id = u.id
            WHERE p.condominium_id = @CondominiumId AND p.deleted_at IS NULL
            ORDER BY p.is_pinned DESC, p.created_at DESC";
        return await QueryAsync<PostDto>(sql, new { CondominiumId = condominiumId });
    }

    public async Task<PostDto?> GetByIdAsync(Guid id)
    {
        var sql = @"
            SELECT p.id, p.condominium_id as CondominiumId, p.author_id as AuthorId, p.title, p.content, 
                   p.category, p.is_pinned as IsPinned, p.is_announcement as IsAnnouncement, 
                   p.status, p.view_count as ViewCount, p.created_at as CreatedAt, p.updated_at as UpdatedAt,
                   u.full_name as AuthorName
            FROM posts p
            JOIN users u ON p.author_id = u.id
            WHERE p.id = @Id AND p.deleted_at IS NULL";
        return await QueryFirstOrDefaultAsync<PostDto>(sql, new { Id = id });
    }

    public async Task<Guid> CreateAsync(CreatePostDto dto, Guid authorId, Guid condominiumId)
    {
        var sql = @"
            INSERT INTO posts (id, condominium_id, author_id, title, content, category, is_pinned, is_announcement, status)
            VALUES (gen_random_uuid(), @CondominiumId, @AuthorId, @Title, @Content, @Category, @IsPinned, @IsAnnouncement, 2)
            RETURNING id";
        return await ExecuteScalarAsync<Guid>(sql, new 
        { 
            CondominiumId = condominiumId, 
            AuthorId = authorId, 
            dto.Title, 
            dto.Content, 
            dto.Category, 
            dto.IsPinned, 
            dto.IsAnnouncement 
        });
    }

    public async Task UpdateAsync(Guid id, UpdatePostDto dto)
    {
        var sql = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        if (!string.IsNullOrEmpty(dto.Title))
        {
            sql.Add("title = @Title");
            parameters.Add("Title", dto.Title);
        }
        if (!string.IsNullOrEmpty(dto.Content))
        {
            sql.Add("content = @Content");
            parameters.Add("Content", dto.Content);
        }
        if (dto.Category.HasValue)
        {
            sql.Add("category = @Category");
            parameters.Add("Category", dto.Category);
        }
        if (dto.IsPinned.HasValue)
        {
            sql.Add("is_pinned = @IsPinned");
            parameters.Add("IsPinned", dto.IsPinned);
        }
        if (dto.IsAnnouncement.HasValue)
        {
            sql.Add("is_announcement = @IsAnnouncement");
            parameters.Add("IsAnnouncement", dto.IsAnnouncement);
        }
        if (dto.Status.HasValue)
        {
            sql.Add("status = @Status");
            parameters.Add("Status", dto.Status);
        }

        if (sql.Count == 0) return;

        sql.Add("updated_at = CURRENT_TIMESTAMP");
        var updateSql = $"UPDATE posts SET {string.Join(", ", sql)} WHERE id = @Id AND deleted_at IS NULL";
        await ExecuteAsync(updateSql, parameters);
    }

    public async Task SoftDeleteAsync(Guid id)
    {
        var sql = "UPDATE posts SET deleted_at IS NOT NULL, deleted_at = CURRENT_TIMESTAMP WHERE id = @Id";
        await ExecuteAsync(sql, new { Id = id });
    }

    public async Task IncrementViewCountAsync(Guid id)
    {
        var sql = "UPDATE posts SET view_count = view_count + 1 WHERE id = @Id";
        await ExecuteAsync(sql, new { Id = id });
    }
}
