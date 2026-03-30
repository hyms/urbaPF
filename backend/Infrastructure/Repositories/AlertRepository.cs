using Dapper;
using UrbaPF.Domain.Entities;
using UrbaPF.Infrastructure.Data;

namespace UrbaPF.Infrastructure.Repositories;

public interface IAlertRepository
{
    Task<IEnumerable<Alert>> GetByCondominiumAsync(Guid condominiumId, int? status = null);
    Task<Alert?> GetByIdAsync(Guid id);
    Task<Alert?> GetByIdWithCreatorAsync(Guid id);
    Task<Guid> CreateAsync(Alert alert);
    Task<bool> UpdateAsync(Alert alert);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> UpdateStatusAsync(Guid id, int status);
    Task<bool> ApproveAsync(Guid id, Guid approverId);
    Task<bool> AcknowledgeAsync(Guid id, Guid userId);
    Task<bool> MarkNotifiedAsync(Guid id);
    Task<bool> ResendNotificationAsync(Guid id);
    Task<IEnumerable<AlertNotification>> GetNotificationsByUserAsync(Guid userId);
    Task<Guid> CreateNotificationAsync(AlertNotification notification);
    Task<bool> MarkNotificationReadAsync(Guid notificationId);
}

public class AlertRepository : BaseRepository, IAlertRepository
{
    public AlertRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<IEnumerable<Alert>> GetByCondominiumAsync(Guid condominiumId, int? status = null)
    {
        var sql = @"SELECT * FROM alerts 
                    WHERE condominium_id = @CondominiumId 
                    AND deleted_at IS NULL";

        if (status.HasValue)
            sql += " AND status = @Status";

        sql += " ORDER BY created_at DESC";

        return await QueryAsync<Alert>(sql, new { CondominiumId = condominiumId, Status = status });
    }

    public async Task<Alert?> GetByIdAsync(Guid id)
    {
        var sql = @"SELECT * FROM alerts WHERE id = @Id AND deleted_at IS NULL";
        return await QueryFirstOrDefaultAsync<Alert>(sql, new { Id = id });
    }

    public async Task<Alert?> GetByIdWithCreatorAsync(Guid id)
    {
        var sql = @"SELECT a.*, u.full_name as CreatorFullName, u.credibility_level as CreatorReputation
                    FROM alerts a
                    LEFT JOIN users u ON a.creator_id = u.id
                    WHERE a.id = @Id AND a.deleted_at IS NULL";
        return await QueryFirstOrDefaultAsync<Alert>(sql, new { Id = id });
    }

    public async Task<Guid> CreateAsync(Alert alert)
    {
        alert.Id = Guid.NewGuid();
        alert.CreatedAt = DateTime.UtcNow;

        var sql = @"INSERT INTO alerts (
            id, condominium_id, creator_id, type, title, description, location,
            status, reputation_level, needs_approval, created_at
        ) VALUES (
            @Id, @CondominiumId, @CreatorId, @Type, @Title, @Description, 
            ST_GeomFromText(@Location, 4326),
            @Status, @ReputationLevel, @NeedsApproval, @CreatedAt
        )";

        await ExecuteAsync(sql, new
        {
            alert.Id,
            alert.CondominiumId,
            alert.CreatorId,
            alert.Type,
            alert.Title,
            alert.Description,
            Location = alert.Location != null ? $"POINT({alert.Location})" : null,
            alert.Status,
            alert.ReputationLevel,
            alert.NeedsApproval,
            alert.CreatedAt
        });

        return alert.Id;
    }

    public async Task<bool> UpdateAsync(Alert alert)
    {
        alert.UpdatedAt = DateTime.UtcNow;

        var sql = @"UPDATE alerts SET
            type = @Type,
            title = @Title,
            description = @Description,
            location = ST_GeomFromText(@Location, 4326),
            status = @Status,
            updated_at = @UpdatedAt
            WHERE id = @Id AND deleted_at IS NULL";

        var result = await ExecuteAsync(sql, new
        {
            alert.Id,
            alert.Type,
            alert.Title,
            alert.Description,
            Location = alert.Location != null ? $"POINT({alert.Location})" : null,
            alert.Status,
            alert.UpdatedAt
        });

        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var sql = @"UPDATE alerts SET deleted_at = CURRENT_TIMESTAMP WHERE id = @Id";
        var result = await ExecuteAsync(sql, new { Id = id });
        return result > 0;
    }

    public async Task<bool> UpdateStatusAsync(Guid id, int status)
    {
        var sql = @"UPDATE alerts SET status = @Status, updated_at = CURRENT_TIMESTAMP WHERE id = @Id";
        var result = await ExecuteAsync(sql, new { Id = id, Status = status });
        return result > 0;
    }

    public async Task<bool> ApproveAsync(Guid id, Guid approverId)
    {
        var sql = @"UPDATE alerts SET 
            status = 2,
            approved_by_id = @ApproverId,
            approved_at = CURRENT_TIMESTAMP,
            updated_at = CURRENT_TIMESTAMP
            WHERE id = @Id";
        var result = await ExecuteAsync(sql, new { Id = id, ApproverId = approverId });
        return result > 0;
    }

    public async Task<bool> AcknowledgeAsync(Guid id, Guid userId)
    {
        var sql = @"UPDATE alerts SET 
            status = 4,
            acknowledged_by_id = @UserId,
            acknowledged_at = CURRENT_TIMESTAMP,
            updated_at = CURRENT_TIMESTAMP
            WHERE id = @Id";
        var result = await ExecuteAsync(sql, new { Id = id, UserId = userId });
        return result > 0;
    }

    public async Task<bool> MarkNotifiedAsync(Guid id)
    {
        var sql = @"UPDATE alerts SET 
            status = 3,
            notified_at = CURRENT_TIMESTAMP,
            updated_at = CURRENT_TIMESTAMP
            WHERE id = @Id";
        var result = await ExecuteAsync(sql, new { Id = id });
        return result > 0;
    }

    public async Task<bool> ResendNotificationAsync(Guid id)
    {
        var sql = @"UPDATE alerts SET 
            notified_at = CURRENT_TIMESTAMP,
            updated_at = CURRENT_TIMESTAMP
            WHERE id = @Id";
        var result = await ExecuteAsync(sql, new { Id = id });
        return result > 0;
    }

    public async Task<IEnumerable<AlertNotification>> GetNotificationsByUserAsync(Guid userId)
    {
        var sql = @"SELECT * FROM alert_notifications 
                    WHERE user_id = @UserId 
                    ORDER BY created_at DESC";
        return await QueryAsync<AlertNotification>(sql, new { UserId = userId });
    }

    public async Task<Guid> CreateNotificationAsync(AlertNotification notification)
    {
        notification.Id = Guid.NewGuid();
        notification.CreatedAt = DateTime.UtcNow;

        var sql = @"INSERT INTO alert_notifications (id, alert_id, user_id, created_at)
                    VALUES (@Id, @AlertId, @UserId, @CreatedAt)";

        await ExecuteAsync(sql, notification);
        return notification.Id;
    }

    public async Task<bool> MarkNotificationReadAsync(Guid notificationId)
    {
        var sql = @"UPDATE alert_notifications SET 
            is_read = true, 
            read_at = CURRENT_TIMESTAMP 
            WHERE id = @Id";
        var result = await ExecuteAsync(sql, new { Id = notificationId });
        return result > 0;
    }
}
