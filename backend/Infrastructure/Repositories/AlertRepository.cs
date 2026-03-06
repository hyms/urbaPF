using Dapper;
using UrbaPF.Infrastructure.Data;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;

namespace UrbaPF.Infrastructure.Repositories;

public class AlertRepository : BaseRepository, IAlertRepository
{
    public AlertRepository(DbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<IEnumerable<AlertDto>> GetAllAsync()
    {
        var sql = @"
            SELECT a.id, a.condominium_id as CondominiumId, a.created_by_id as CreatedById, a.alert_type as AlertType,
                   a.message, a.latitude, a.longitude, a.destination_address as DestinationAddress,
                   a.estimated_arrival as EstimatedArrival, a.status, a.acknowledged_at as AcknowledgedAt,
                   a.arrived_at as ArrivedAt, a.created_at as CreatedAt, a.updated_at as UpdatedAt,
                   u.full_name as CreatedByName
            FROM alerts a
            JOIN users u ON a.created_by_id = u.id
            WHERE a.deleted_at IS NULL
            ORDER BY a.created_at DESC";
        return await QueryAsync<AlertDto>(sql);
    }

    public async Task<IEnumerable<AlertDto>> GetActiveByCondominiumAsync(Guid condominiumId)
    {
        var sql = @"
            SELECT a.id, a.condominium_id as CondominiumId, a.created_by_id as CreatedById, a.alert_type as AlertType,
                   a.message, a.latitude, a.longitude, a.destination_address as DestinationAddress,
                   a.estimated_arrival as EstimatedArrival, a.status, a.acknowledged_at as AcknowledgedAt,
                   a.arrived_at as ArrivedAt, a.created_at as CreatedAt, a.updated_at as UpdatedAt,
                   u.full_name as CreatedByName
            FROM alerts a
            JOIN users u ON a.created_by_id = u.id
            WHERE a.condominium_id = @CondominiumId 
              AND a.status NOT IN (5, 6)
              AND a.deleted_at IS NULL
            ORDER BY a.created_at DESC";
        return await QueryAsync<AlertDto>(sql, new { CondominiumId = condominiumId });
    }

    public async Task<AlertDto?> GetByIdAsync(Guid id)
    {
        var sql = @"
            SELECT a.id, a.condominium_id as CondominiumId, a.created_by_id as CreatedById, a.alert_type as AlertType,
                   a.message, a.latitude, a.longitude, a.destination_address as DestinationAddress,
                   a.estimated_arrival as EstimatedArrival, a.status, a.acknowledged_at as AcknowledgedAt,
                   a.arrived_at as ArrivedAt, a.created_at as CreatedAt, a.updated_at as UpdatedAt,
                   u.full_name as CreatedByName
            FROM alerts a
            JOIN users u ON a.created_by_id = u.id
            WHERE a.id = @Id AND a.deleted_at IS NULL";
        return await QueryFirstOrDefaultAsync<AlertDto>(sql, new { Id = id });
    }

    public async Task<Guid> CreateAsync(CreateAlertDto dto, Guid userId, Guid condominiumId)
    {
        var sql = @"
            INSERT INTO alerts (condominium_id, created_by_id, alert_type, message, latitude, longitude,
                              destination_address, estimated_arrival, status)
            VALUES (@CondominiumId, @CreatedById, @AlertType, @Message, @Latitude, @Longitude,
                   @DestinationAddress, @EstimatedArrival, 1)
            RETURNING id";
        return await ExecuteScalarAsync<Guid>(sql, new 
        { 
            CondominiumId = condominiumId,
            CreatedById = userId,
            dto.AlertType,
            dto.Message,
            dto.Latitude,
            dto.Longitude,
            dto.DestinationAddress,
            dto.EstimatedArrival
        });
    }

    public async Task UpdateAsync(Guid id, UpdateAlertDto dto)
    {
        var sql = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        if (!string.IsNullOrEmpty(dto.Message))
        {
            sql.Add("message = @Message");
            parameters.Add("Message", dto.Message);
        }
        if (dto.AlertType.HasValue)
        {
            sql.Add("alert_type = @AlertType");
            parameters.Add("AlertType", dto.AlertType);
        }
        if (dto.Latitude.HasValue)
        {
            sql.Add("latitude = @Latitude");
            parameters.Add("Latitude", dto.Latitude);
        }
        if (dto.Longitude.HasValue)
        {
            sql.Add("longitude = @Longitude");
            parameters.Add("Longitude", dto.Longitude);
        }
        if (dto.DestinationAddress is not null)
        {
            sql.Add("destination_address = @DestinationAddress");
            parameters.Add("DestinationAddress", dto.DestinationAddress);
        }
        if (dto.EstimatedArrival.HasValue)
        {
            sql.Add("estimated_arrival = @EstimatedArrival");
            parameters.Add("EstimatedArrival", dto.EstimatedArrival);
        }

        if (sql.Count == 0) return;

        sql.Add("updated_at = CURRENT_TIMESTAMP");

        var updateSql = $"UPDATE alerts SET {string.Join(", ", sql)} WHERE id = @Id AND deleted_at IS NULL";
        await ExecuteAsync(updateSql, parameters);
    }

    public async Task UpdateStatusAsync(Guid id, int status)
    {
        var sql = status switch
        {
            2 => "UPDATE alerts SET status = @Status, acknowledged_at = CURRENT_TIMESTAMP WHERE id = @Id",
            4 => "UPDATE alerts SET status = @Status, arrived_at = CURRENT_TIMESTAMP WHERE id = @Id",
            _ => "UPDATE alerts SET status = @Status WHERE id = @Id"
        };
        await ExecuteAsync(sql, new { Id = id, Status = status });
    }

    public async Task SoftDeleteAsync(Guid id)
    {
        var sql = "UPDATE alerts SET deleted_at IS NOT NULL, deleted_at = CURRENT_TIMESTAMP WHERE id = @Id";
        await ExecuteAsync(sql, new { Id = id });
    }
}
