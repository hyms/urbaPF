using UrbaPF.Domain.Entities;
using UrbaPF.Domain.Services;
using UrbaPF.Infrastructure.Repositories;
using UrbaPF.Infrastructure.Interfaces;

namespace UrbaPF.Infrastructure.Services;

public interface IAlertService
{
    Task<IEnumerable<Alert>> GetByCondominiumAsync(Guid condominiumId, int? status = null);
    Task<Alert?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(Guid condominiumId, Guid creatorId, int creatorReputation, CreateAlertRequest request);
    Task<bool> ApproveAsync(Guid alertId, Guid approverId);
    Task<bool> AcknowledgeAsync(Guid alertId, Guid userId);
    Task<bool> ResolveAsync(Guid alertId, Guid userId);
    Task<bool> DeleteAsync(Guid alertId, Guid userId, int userRole);
    Task<bool> ResendNotificationAsync(Guid alertId, Guid userId, int userRole);
    Task<IEnumerable<Alert>> GetActiveAlertsAsync(Guid condominiumId);
}

public record CreateAlertRequest(
    int Type,
    string Title,
    string? Description,
    string? Location
);

public class AlertService : IAlertService
{
    private readonly IAlertRepository _alertRepository;
    private readonly IUserRepository _userRepository;
    private readonly AlertDomainService _domainService;

    public AlertService(
        IAlertRepository alertRepository,
        IUserRepository userRepository,
        AlertDomainService domainService)
    {
        _alertRepository = alertRepository;
        _userRepository = userRepository;
        _domainService = domainService;
    }

    public async Task<IEnumerable<Alert>> GetByCondominiumAsync(Guid condominiumId, int? status = null)
    {
        return await _alertRepository.GetByCondominiumAsync(condominiumId, status);
    }

    public async Task<Alert?> GetByIdAsync(Guid id)
    {
        return await _alertRepository.GetByIdAsync(id);
    }

    public async Task<Guid> CreateAsync(Guid condominiumId, Guid creatorId, int creatorReputation, CreateAlertRequest request)
    {
        var needsApproval = _domainService.ShouldRequireApproval(creatorReputation);

        var alert = new Alert
        {
            CondominiumId = condominiumId,
            CreatorId = creatorId,
            Type = request.Type,
            Title = request.Title,
            Description = request.Description,
            Location = request.Location,
            Status = 1,
            ReputationLevel = creatorReputation,
            NeedsApproval = needsApproval
        };

        return await _alertRepository.CreateAsync(alert);
    }

    public async Task<bool> ApproveAsync(Guid alertId, Guid approverId)
    {
        var alert = await _alertRepository.GetByIdAsync(alertId);
        if (alert == null) return false;

        if (alert.Status != 1)
            return false;

        var success = await _alertRepository.ApproveAsync(alertId, approverId);
        if (success)
        {
            await _alertRepository.MarkNotifiedAsync(alertId);
        }

        return success;
    }

    public async Task<bool> AcknowledgeAsync(Guid alertId, Guid userId)
    {
        var alert = await _alertRepository.GetByIdAsync(alertId);
        if (alert == null) return false;

        if (!_domainService.CanAcknowledge(3))
            return false;

        return await _alertRepository.AcknowledgeAsync(alertId, userId);
    }

    public async Task<bool> ResolveAsync(Guid alertId, Guid userId)
    {
        var alert = await _alertRepository.GetByIdAsync(alertId);
        if (alert == null) return false;

        if (!_domainService.CanResolve(3, alert.CreatorId == userId))
            return false;

        return await _alertRepository.UpdateStatusAsync(alertId, 5);
    }

    public async Task<bool> DeleteAsync(Guid alertId, Guid userId, int userRole)
    {
        var alert = await _alertRepository.GetByIdAsync(alertId);
        if (alert == null) return false;

        if (!_domainService.CanDelete(userRole, alert.CreatorId == userId))
            return false;

        return await _alertRepository.DeleteAsync(alertId);
    }

    public async Task<bool> ResendNotificationAsync(Guid alertId, Guid userId, int userRole)
    {
        var alert = await _alertRepository.GetByIdAsync(alertId);
        if (alert == null) return false;

        if (!_domainService.CanResendNotification(userRole, alert.CreatorId == userId, alert.Status))
            return false;

        if (alert.NeedsApproval && alert.Status < 2)
            return false;

        return await _alertRepository.ResendNotificationAsync(alertId);
    }

    public async Task<IEnumerable<Alert>> GetActiveAlertsAsync(Guid condominiumId)
    {
        var alerts = await _alertRepository.GetByCondominiumAsync(condominiumId, null);
        return alerts.Where(a => _domainService.IsAlertActive(a.Status));
    }
}
