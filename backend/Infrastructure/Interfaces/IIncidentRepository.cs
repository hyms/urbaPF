using UrbaPF.Infrastructure.DTOs;

namespace UrbaPF.Infrastructure.Interfaces;

public interface IIncidentRepository
{
    Task<IEnumerable<IncidentDto>> GetAllAsync();
    Task<IncidentDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<IncidentDto>> GetByCondominiumAsync(Guid condominiumId, int? status = null);
    Task<Guid> CreateAsync(CreateIncidentDto dto, Guid userId, Guid condominiumId);
    Task UpdateAsync(Guid id, UpdateIncidentDto dto);
    Task UpdateStatusAsync(Guid id, int status, string? resolutionNotes);
    Task SoftDeleteAsync(Guid id);
}
