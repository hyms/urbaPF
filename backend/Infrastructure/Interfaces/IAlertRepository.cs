using UrbaPF.Domain.Entities;

namespace UrbaPF.Infrastructure.Interfaces;

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
