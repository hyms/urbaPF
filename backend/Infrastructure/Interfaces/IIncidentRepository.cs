using UrbaPF.Domain.Entities;

namespace UrbaPF.Infrastructure.Interfaces;

public interface IIncidentRepository
{
    Task<IEnumerable<Incident>> GetByCondominiumAsync(Guid condominiumId, int? status = null);
    Task<Incident?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(Incident incident);
    Task<bool> UpdateAsync(Incident incident);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> UpdateStatusAsync(Guid id, int status, string? resolutionNotes = null);
}
