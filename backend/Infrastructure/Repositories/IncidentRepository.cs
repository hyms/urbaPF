using Dapper;
using UrbaPF.Infrastructure.Data;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;

namespace UrbaPF.Infrastructure.Repositories;

public class IncidentRepository : BaseRepository, IIncidentRepository
{
    public IncidentRepository(DbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<IEnumerable<IncidentDto>> GetAllAsync()
    {
        var sql = @"
            SELECT i.id, i.condominium_id as CondominiumId, i.reported_by_id as ReportedById, i.title, 
                   i.description, i.type, i.priority, i.status, i.latitude, i.longitude, 
                   i.location_description as LocationDescription, i.occurred_at as OccurredAt,
                   i.resolved_at as ResolvedAt, i.resolution_notes as ResolutionNotes,
                   i.created_at as CreatedAt, i.updated_at as UpdatedAt,
                   u.full_name as ReportedByName
            FROM incidents i
            JOIN users u ON i.reported_by_id = u.id
            WHERE i.deleted_at IS NULL
            ORDER BY i.priority DESC, i.created_at DESC";
        return await QueryAsync<IncidentDto>(sql);
    }

    public async Task<IEnumerable<IncidentDto>> GetByCondominiumAsync(Guid condominiumId, int? status = null)
    {
        var sql = @"
            SELECT i.id, i.condominium_id as CondominiumId, i.reported_by_id as ReportedById, i.title, 
                   i.description, i.type, i.priority, i.status, i.latitude, i.longitude, 
                   i.location_description as LocationDescription, i.occurred_at as OccurredAt,
                   i.resolved_at as ResolvedAt, i.resolution_notes as ResolutionNotes,
                   i.created_at as CreatedAt, i.updated_at as UpdatedAt,
                   u.full_name as ReportedByName
            FROM incidents i
            JOIN users u ON i.reported_by_id = u.id
            WHERE i.condominium_id = @CondominiumId AND i.deleted_at IS NULL";
        
        if (status.HasValue)
            sql += " AND status = @Status";
        
        sql += " ORDER BY i.priority DESC, i.created_at DESC";
        
        return await QueryAsync<IncidentDto>(sql, new { CondominiumId = condominiumId, Status = status });
    }

    public async Task<IncidentDto?> GetByIdAsync(Guid id)
    {
        var sql = @"
            SELECT i.id, i.condominium_id as CondominiumId, i.reported_by_id as ReportedById, i.title, 
                   i.description, i.type, i.priority, i.status, i.latitude, i.longitude, 
                   i.location_description as LocationDescription, i.occurred_at as OccurredAt,
                   i.resolved_at as ResolvedAt, i.resolution_notes as ResolutionNotes,
                   i.created_at as CreatedAt, i.updated_at as UpdatedAt,
                   u.full_name as ReportedByName
            FROM incidents i
            JOIN users u ON i.reported_by_id = u.id
            WHERE i.id = @Id AND i.deleted_at IS NULL";
        return await QueryFirstOrDefaultAsync<IncidentDto>(sql, new { Id = id });
    }

    public async Task<Guid> CreateAsync(CreateIncidentDto dto, Guid reportedById, Guid condominiumId)
    {
        var sql = @"
            INSERT INTO incidents (condominium_id, reported_by_id, title, description, type, priority, status,
                                 latitude, longitude, location_description, occurred_at)
            VALUES (@CondominiumId, @ReportedById, @Title, @Description, @Type, @Priority, 1,
                   @Latitude, @Longitude, @LocationDescription, @OccurredAt)
            RETURNING id";
        return await ExecuteScalarAsync<Guid>(sql, new 
        { 
            CondominiumId = condominiumId,
            ReportedById = reportedById,
            dto.Title,
            dto.Description,
            dto.Type,
            dto.Priority,
            dto.Latitude,
            dto.Longitude,
            dto.LocationDescription,
            dto.OccurredAt
        });
    }

    public async Task UpdateAsync(Guid id, UpdateIncidentDto dto)
    {
        var sql = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        if (!string.IsNullOrEmpty(dto.Title))
        {
            sql.Add("title = @Title");
            parameters.Add("Title", dto.Title);
        }
        if (!string.IsNullOrEmpty(dto.Description))
        {
            sql.Add("description = @Description");
            parameters.Add("Description", dto.Description);
        }
        if (dto.Type.HasValue)
        {
            sql.Add("type = @Type");
            parameters.Add("Type", dto.Type);
        }
        if (dto.Priority.HasValue)
        {
            sql.Add("priority = @Priority");
            parameters.Add("Priority", dto.Priority);
        }

        if (sql.Count == 0) return;

        sql.Add("updated_at = CURRENT_TIMESTAMP");

        var updateSql = $"UPDATE incidents SET {string.Join(", ", sql)} WHERE id = @Id AND deleted_at IS NULL";
        await ExecuteAsync(updateSql, parameters);
    }

    public async Task UpdateStatusAsync(Guid id, int status, string? resolutionNotes = null)
    {
        var sql = status == 4 
            ? "UPDATE incidents SET status = @Status, resolved_at = CURRENT_TIMESTAMP, resolution_notes = @ResolutionNotes WHERE id = @Id"
            : "UPDATE incidents SET status = @Status WHERE id = @Id";
        await ExecuteAsync(sql, new { Id = id, Status = status, ResolutionNotes = resolutionNotes });
    }

    public async Task SoftDeleteAsync(Guid id)
    {
        var sql = "UPDATE incidents SET deleted_at IS NOT NULL, deleted_at = CURRENT_TIMESTAMP WHERE id = @Id";
        await ExecuteAsync(sql, new { Id = id });
    }
}
