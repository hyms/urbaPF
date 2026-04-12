using UrbaPF.Domain.Entities;
using UrbaPF.Domain.Services;
using UrbaPF.Infrastructure.Repositories;
using UrbaPF.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using UrbaPF.Domain.Enums;

namespace UrbaPF.Infrastructure.Services;

public interface IIncidentService
{
    Task<IEnumerable<Incident>> GetByCondominiumAsync(Guid condominiumId, int? status = null);
    Task<Incident?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(Guid condominiumId, Guid reporterId, CreateIncidentRequest request, List<IFormFile>? mediaFiles = null);
    Task<bool> UpdateAsync(Guid id, Guid userId, UserRole userRole, UpdateIncidentRequest request, List<IFormFile>? mediaFiles = null);
    Task<bool> UpdateStatusAsync(Guid id, UserRole userRole, bool isReporter, int newStatus, string? resolutionNotes = null);
    Task<bool> DeleteAsync(Guid id, Guid userId, UserRole userRole);
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
    private readonly IAuditService _auditService;
    private readonly IFileStorageService _fileStorageService;

    public IncidentService(
        IIncidentRepository incidentRepository, 
        IncidentDomainService domainService,
        IAuditService auditService,
        IFileStorageService fileStorageService)
    {
        _incidentRepository = incidentRepository;
        _domainService = domainService;
        _auditService = auditService;
        _fileStorageService = fileStorageService;
    }

    public async Task<IEnumerable<Incident>> GetByCondominiumAsync(Guid condominiumId, int? status = null)
    {
        return await _incidentRepository.GetByCondominiumAsync(condominiumId, status);
    }

    public async Task<Incident?> GetByIdAsync(Guid id)
    {
        return await _incidentRepository.GetByIdAsync(id);
    }

    public async Task<Guid> CreateAsync(Guid condominiumId, Guid reporterId, CreateIncidentRequest request, List<IFormFile>? mediaFiles = null)
    {
        var priority = _domainService.CalculatePriority(request.Type, request.Description);
        List<IncidentMedia>? mediaPaths = null;

        if (mediaFiles != null && mediaFiles.Any())
        {
            mediaPaths = new List<IncidentMedia>();
            foreach (var file in mediaFiles)
            {
                var filePath = await _fileStorageService.UploadFileAsync(file, "incidents");
                if (filePath != null) {
                    mediaPaths.Add(new IncidentMedia { Type = file.ContentType, Path = filePath });
                }
            }
        }

        var incident = new Incident
        {
            CondominiumId = condominiumId,
            ReporterId = reporterId,
            Title = request.Title,
            Description = request.Description,
            Type = request.Type,
            Priority = priority,
            Status = (int)IncidentStatus.Reported,
            Location = request.Location,
            AddressReference = request.AddressReference,
            Media = mediaPaths != null ? _domainService.SerializeMedia(mediaPaths) : null
        };

        var incidentId = await _incidentRepository.CreateAsync(incident);
        await _auditService.LogEventAsync(reporterId, condominiumId, "INCIDENT_CREATED", incidentId, request);
        return incidentId;
    }

    public async Task<bool> UpdateAsync(Guid id, Guid userId, UserRole userRole, UpdateIncidentRequest request, List<IFormFile>? mediaFiles = null)
    {
        var incident = await _incidentRepository.GetByIdAsync(id);
        if (incident == null)
            return false;

        if (!_domainService.CanEdit(incident, userRole, incident.ReporterId == userId))
            return false;

        // Handle media updates: for now, replace all media if new files are provided
        List<IncidentMedia>? newMediaPaths = null;
        if (mediaFiles != null && mediaFiles.Any())
        {
            newMediaPaths = new List<IncidentMedia>();
            foreach (var file in mediaFiles)
            {
                var filePath = await _fileStorageService.UploadFileAsync(file, "incidents");
                if (filePath != null) {
                    newMediaPaths.Add(new IncidentMedia { Type = file.ContentType, Path = filePath });
                }
            }
            // Delete old files if implementing a full replacement strategy
            // For MVP, just updating the JSON in DB
        } else if (request.Media != null) {
            newMediaPaths = request.Media;
        } else if (incident.Media != null) {
            // If no new files and request.Media is null, but incident had media, clear it
            newMediaPaths = new List<IncidentMedia>();
        }

        incident.Title = request.Title;
        incident.Description = request.Description;
        incident.Type = request.Type;
        incident.Location = request.Location;
        incident.AddressReference = request.AddressReference;
        incident.Media = newMediaPaths != null ? _domainService.SerializeMedia(newMediaPaths) : null;

        var success = await _incidentRepository.UpdateAsync(incident);
        if (success)
        {
            await _auditService.LogEventAsync(userId, incident.CondominiumId, "INCIDENT_UPDATED", id, request);
        }
        return success;
    }

    public async Task<bool> UpdateStatusAsync(Guid id, UserRole userRole, bool isReporter, int newStatus, string? resolutionNotes = null)
    {
        var incident = await _incidentRepository.GetByIdAsync(id);
        if (incident == null)
            return false;

        if (!_domainService.CanChangeStatus(incident.Status, newStatus, userRole))
            return false;

        if (newStatus == (int)IncidentStatus.Closed && !_domainService.CanClose(incident.Status, userRole, isReporter))
            return false;

        var success = await _incidentRepository.UpdateStatusAsync(id, newStatus, resolutionNotes);
        if (success)
        {
            await _auditService.LogEventAsync(Guid.Empty, incident.CondominiumId, "INCIDENT_STATUS_UPDATED", id, new { newStatus, resolutionNotes });
        }
        return success;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, UserRole userRole)
    {
        var incident = await _incidentRepository.GetByIdAsync(id);
        if (incident == null)
            return false;

        if (userRole < UserRole.Administrator && incident.ReporterId != userId)
            return false;

        if (incident.Status >= (int)IncidentStatus.Resolved)
            return false;

        var success = await _incidentRepository.DeleteAsync(id);
        if (success)
        {
            await _auditService.LogEventAsync(userId, incident.CondominiumId, "INCIDENT_DELETED", id, new { });
        }
        return success;
    }
}
