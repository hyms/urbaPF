using Dapper;
using UrbaPF.Infrastructure.Data;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;

namespace UrbaPF.Infrastructure.Repositories;

public class CommentRepository : BaseRepository, ICommentRepository
{
    public CommentRepository(DbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<IEnumerable<CommentDto>> GetByPostAsync(Guid postId)
    {
        var sql = @"
            SELECT c.id, c.post_id as PostId, c.author_id as AuthorId, c.parent_comment_id as ParentCommentId,
                   c.content, c.credibility_level as CredibilityLevel, c.created_at as CreatedAt,
                   u.full_name as AuthorName
            FROM comments c
            JOIN users u ON c.author_id = u.id
            WHERE c.post_id = @PostId AND c.deleted_at IS NULL
            ORDER BY c.credibility_level DESC, c.created_at DESC";
        return await QueryAsync<CommentDto>(sql, new { PostId = postId });
    }

    public async Task<CommentDto?> GetByIdAsync(Guid id)
    {
        var sql = @"
            SELECT c.id, c.post_id as PostId, c.author_id as AuthorId, c.parent_comment_id as ParentCommentId,
                   c.content, c.credibility_level as CredibilityLevel, c.created_at as CreatedAt,
                   u.full_name as AuthorName
            FROM comments c
            JOIN users u ON c.author_id = u.id
            WHERE c.id = @Id AND c.deleted_at IS NULL";
        return await QueryFirstOrDefaultAsync<CommentDto>(sql, new { Id = id });
    }

    public async Task<Guid> CreateAsync(CreateCommentDto dto, Guid authorId, Guid postId)
    {
        var sql = @"
            INSERT INTO comments (id, post_id, author_id, parent_comment_id, content, credibility_level)
            VALUES (gen_random_uuid(), @PostId, @AuthorId, @ParentCommentId, @Content, 
                    (SELECT credibility_level FROM users WHERE id = @AuthorId))
            RETURNING id";
        return await ExecuteScalarAsync<Guid>(sql, new 
        { 
            PostId = postId, 
            AuthorId = authorId, 
            dto.ParentCommentId, 
            dto.Content 
        });
    }

    public async Task SoftDeleteAsync(Guid id)
    {
        var sql = "UPDATE comments SET deleted_at = CURRENT_TIMESTAMP WHERE id = @Id";
        await ExecuteAsync(sql, new { Id = id });
    }
}
