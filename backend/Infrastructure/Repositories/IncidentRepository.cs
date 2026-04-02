using Dapper;
using UrbaPF.Domain.Entities;
using UrbaPF.Infrastructure.Data;
using UrbaPF.Infrastructure.Interfaces;

namespace UrbaPF.Infrastructure.Repositories;

public class IncidentRepository : BaseRepository, IIncidentRepository
{
    public IncidentRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<IEnumerable<Incident>> GetByCondominiumAsync(Guid condominiumId, int? status = null)
    {
        var sql = @"SELECT * FROM incidents 
                    WHERE condominium_id = @CondominiumId 
                    AND deleted_at IS NULL";

        if (status.HasValue)
            sql += " AND status = @Status";

        sql += " ORDER BY created_at DESC";

        return await QueryAsync<Incident>(sql, new { CondominiumId = condominiumId, Status = status });
    }

    public async Task<Incident?> GetByIdAsync(Guid id)
    {
        var sql = @"SELECT * FROM incidents WHERE id = @Id AND deleted_at IS NULL";
        return await QueryFirstOrDefaultAsync<Incident>(sql, new { Id = id });
    }

    public async Task<Guid> CreateAsync(Incident incident)
    {
        incident.Id = Guid.NewGuid();
        incident.CreatedAt = DateTime.UtcNow;

        var sql = @"INSERT INTO incidents (
            id, condominium_id, reporter_id, title, description, type, priority, status,
            location, address_reference, media, created_at
        ) VALUES (
            @Id, @CondominiumId, @ReporterId, @Title, @Description, @Type, @Priority, @Status,
            ST_GeomFromText(@Location, 4326), @AddressReference, @Media, @CreatedAt
        )";

        await ExecuteAsync(sql, new
        {
            incident.Id,
            incident.CondominiumId,
            incident.ReporterId,
            incident.Title,
            incident.Description,
            incident.Type,
            incident.Priority,
            incident.Status,
            Location = incident.Location != null ? $"POINT({incident.Location})" : null,
            incident.AddressReference,
            incident.Media,
            incident.CreatedAt
        });

        return incident.Id;
    }

    public async Task<bool> UpdateAsync(Incident incident)
    {
        incident.UpdatedAt = DateTime.UtcNow;

        var sql = @"UPDATE incidents SET
            title = @Title,
            description = @Description,
            type = @Type,
            priority = @Priority,
            status = @Status,
            location = ST_GeomFromText(@Location, 4326),
            address_reference = @AddressReference,
            media = @Media,
            updated_at = @UpdatedAt
            WHERE id = @Id AND deleted_at IS NULL";

        var result = await ExecuteAsync(sql, new
        {
            incident.Id,
            incident.Title,
            incident.Description,
            incident.Type,
            incident.Priority,
            incident.Status,
            Location = incident.Location != null ? $"POINT({incident.Location})" : null,
            incident.AddressReference,
            incident.Media,
            incident.UpdatedAt
        });

        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var sql = @"UPDATE incidents SET deleted_at = CURRENT_TIMESTAMP WHERE id = @Id";
        var result = await ExecuteAsync(sql, new { Id = id });
        return result > 0;
    }

    public async Task<bool> UpdateStatusAsync(Guid id, int status, string? resolutionNotes = null)
    {
        var sql = @"UPDATE incidents SET 
            status = @Status,
            updated_at = CURRENT_TIMESTAMP";

        if (status == 4)
            sql += ", resolved_at = CURRENT_TIMESTAMP";
        else if (status == 5)
            sql += ", closed_at = CURRENT_TIMESTAMP";

        if (!string.IsNullOrEmpty(resolutionNotes))
            sql += ", resolution_notes = @ResolutionNotes";

        sql += " WHERE id = @Id AND deleted_at IS NULL";

        var result = await ExecuteAsync(sql, new { Id = id, Status = status, ResolutionNotes = resolutionNotes });
        return result > 0;
    }
}
