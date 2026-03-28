using Dapper;
using UrbaPF.Infrastructure.Data;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;

namespace UrbaPF.Infrastructure.Repositories;

public class PollRepository : BaseRepository, IPollRepository
{
    public PollRepository(DbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<IEnumerable<PollDto>> GetAllAsync()
    {
        var sql = @"
            SELECT p.id, p.condominium_id as CondominiumId, p.created_by_id as CreatedById, p.title, 
                   p.description, p.options, p.poll_type as PollType, p.starts_at as StartsAt, 
                   p.ends_at as EndsAt, p.requires_justification as RequiresJustification, 
                   p.min_role_to_vote as MinRoleToVote, p.status, p.created_at as CreatedAt, 
                   p.updated_at as UpdatedAt,
                   u.full_name as CreatedByName
            FROM polls p
            JOIN users u ON p.created_by_id = u.id
            WHERE p.deleted_at IS NULL
            ORDER BY p.created_at DESC";
        return await QueryAsync<PollDto>(sql);
    }

    public async Task<IEnumerable<PollDto>> GetByCondominiumAsync(Guid condominiumId)
    {
        var sql = @"
            SELECT p.id, p.condominium_id as CondominiumId, p.created_by_id as CreatedById, p.title, 
                   p.description, p.options, p.poll_type as PollType, p.starts_at as StartsAt, 
                   p.ends_at as EndsAt, p.requires_justification as RequiresJustification, 
                   p.min_role_to_vote as MinRoleToVote, p.status, p.created_at as CreatedAt, 
                   p.updated_at as UpdatedAt,
                   u.full_name as CreatedByName
            FROM polls p
            JOIN users u ON p.created_by_id = u.id
            WHERE p.condominium_id = @CondominiumId AND deleted_at IS NULL
            ORDER BY p.created_at DESC";
        return await QueryAsync<PollDto>(sql, new { CondominiumId = condominiumId });
    }

    public async Task<PollDto?> GetByIdAsync(Guid id)
    {
        var sql = @"
            SELECT p.id, p.condominium_id as CondominiumId, p.created_by_id as CreatedById, p.title, 
                   p.description, p.options, p.poll_type as PollType, p.starts_at as StartsAt, 
                   p.ends_at as EndsAt, p.requires_justification as RequiresJustification, 
                   p.min_role_to_vote as MinRoleToVote, p.status, p.created_at as CreatedAt, 
                   p.updated_at as UpdatedAt,
                   u.full_name as CreatedByName
            FROM polls p
            JOIN users u ON p.created_by_id = u.id
            WHERE p.id = @Id AND p.deleted_at IS NULL";
        return await QueryFirstOrDefaultAsync<PollDto>(sql, new { Id = id });
    }

    public async Task<Guid> CreateAsync(CreatePollDto dto, Guid userId, Guid condominiumId)
    {
        var serverSecret = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        
        var sql = @"
            INSERT INTO polls (id, condominium_id, title, description, options, poll_type, starts_at, ends_at,
                              requires_justification, min_role_to_vote, server_secret, status, created_by_id)
            VALUES (gen_random_uuid(), @CondominiumId, @Title, @Description, @Options::jsonb, @PollType, @StartsAt, @EndsAt,
                   @RequiresJustification, @MinRoleToVote, @ServerSecret, 2, @CreatedById)
            RETURNING id";
        return await ExecuteScalarAsync<Guid>(sql, new 
        { 
            CondominiumId = condominiumId,
            dto.Title,
            dto.Description,
            dto.Options,
            dto.PollType,
            dto.StartsAt,
            dto.EndsAt,
            dto.RequiresJustification,
            dto.MinRoleToVote,
            ServerSecret = serverSecret,
            CreatedById = userId
        });
    }

    public async Task UpdateAsync(Guid id, UpdatePollDto dto)
    {
        var sql = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        if (!string.IsNullOrEmpty(dto.Title))
        {
            sql.Add("title = @Title");
            parameters.Add("Title", dto.Title);
        }
        if (dto.Description is not null)
        {
            sql.Add("description = @Description");
            parameters.Add("Description", dto.Description);
        }
        if (dto.Options is not null)
        {
            sql.Add("options = @Options");
            parameters.Add("Options", dto.Options);
        }
        if (dto.StartsAt.HasValue)
        {
            sql.Add("starts_at = @StartsAt");
            parameters.Add("StartsAt", dto.StartsAt);
        }
        if (dto.EndsAt.HasValue)
        {
            sql.Add("ends_at = @EndsAt");
            parameters.Add("EndsAt", dto.EndsAt);
        }
        if (dto.Status.HasValue)
        {
            sql.Add("status = @Status");
            parameters.Add("Status", dto.Status);
        }

        if (sql.Count == 0) return;

        sql.Add("updated_at = CURRENT_TIMESTAMP");

        var updateSql = $"UPDATE polls SET {string.Join(", ", sql)} WHERE id = @Id AND deleted_at IS NULL";
        await ExecuteAsync(updateSql, parameters);
    }

    public async Task SoftDeleteAsync(Guid id)
    {
        var sql = "UPDATE polls SET deleted_at IS NOT NULL, deleted_at = CURRENT_TIMESTAMP WHERE id = @Id";
        await ExecuteAsync(sql, new { Id = id });
    }
}
