using UrbaPF.Domain.Entities;
using UrbaPF.Domain.Services;
using UrbaPF.Infrastructure.Repositories;

namespace UrbaPF.Infrastructure.Services;

public interface IIncidentService
{
    Task<IEnumerable<Incident>> GetByCondominiumAsync(Guid condominiumId, int? status = null);
    Task<Incident?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(Guid condominiumId, Guid reporterId, CreateIncidentRequest request);
    Task<bool> UpdateAsync(Guid id, Guid userId, int userRole, UpdateIncidentRequest request);
    Task<bool> UpdateStatusAsync(Guid id, int userRole, bool isReporter, int newStatus, string? resolutionNotes = null);
    Task<bool> DeleteAsync(Guid id, Guid userId, int userRole);
}

public record CreateIncidentRequest(
    string Title,
    string? Description,
    int Type,
    string? Location,
    string? AddressReference,
    List<IncidentMedia>? Media
);

public record UpdateIncidentRequest(
    string Title,
    string? Description,
    int Type,
    string? Location,
    string? AddressReference,
    List<IncidentMedia>? Media
);

public class IncidentService : IIncidentService
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly IncidentDomainService _domainService;

    public IncidentService(IIncidentRepository incidentRepository, IncidentDomainService domainService)
    {
        _incidentRepository = incidentRepository;
        _domainService = domainService;
    }

    public async Task<IEnumerable<Incident>> GetByCondominiumAsync(Guid condominiumId, int? status = null)
    {
        return await _incidentRepository.GetByCondominiumAsync(condominiumId, status);
    }

    public async Task<Incident?> GetByIdAsync(Guid id)
    {
        return await _incidentRepository.GetByIdAsync(id);
    }

    public async Task<Guid> CreateAsync(Guid condominiumId, Guid reporterId, CreateIncidentRequest request)
    {
        var priority = _domainService.CalculatePriority(request.Type, request.Description);

        var incident = new Incident
        {
            CondominiumId = condominiumId,
            ReporterId = reporterId,
            Title = request.Title,
            Description = request.Description,
            Type = request.Type,
            Priority = priority,
            Status = 1,
            Location = request.Location,
            AddressReference = request.AddressReference,
            Media = request.Media != null ? _domainService.SerializeMedia(request.Media) : null
        };

        return await _incidentRepository.CreateAsync(incident);
    }

    public async Task<bool> UpdateAsync(Guid id, Guid userId, int userRole, UpdateIncidentRequest request)
    {
        var incident = await _incidentRepository.GetByIdAsync(id);
        if (incident == null)
            return false;

        if (!_domainService.CanEdit(incident, userRole, incident.ReporterId == userId))
            return false;

        incident.Title = request.Title;
        incident.Description = request.Description;
        incident.Type = request.Type;
        incident.Location = request.Location;
        incident.AddressReference = request.AddressReference;
        incident.Media = request.Media != null ? _domainService.SerializeMedia(request.Media) : null;

        return await _incidentRepository.UpdateAsync(incident);
    }

    public async Task<bool> UpdateStatusAsync(Guid id, int userRole, bool isReporter, int newStatus, string? resolutionNotes = null)
    {
        var incident = await _incidentRepository.GetByIdAsync(id);
        if (incident == null)
            return false;

        if (!_domainService.CanChangeStatus(incident.Status, newStatus, userRole))
            return false;

        if (newStatus == 5 && !_domainService.CanClose(incident.Status, userRole, isReporter))
            return false;

        return await _incidentRepository.UpdateStatusAsync(id, newStatus, resolutionNotes);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, int userRole)
    {
        var incident = await _incidentRepository.GetByIdAsync(id);
        if (incident == null)
            return false;

        if (userRole < 4 && incident.ReporterId != userId)
            return false;

        if (incident.Status >= 4)
            return false;

        return await _incidentRepository.DeleteAsync(id);
    }
}
