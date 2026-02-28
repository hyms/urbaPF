using UrbaPF.Infrastructure.DTOs;

namespace UrbaPF.Infrastructure.Interfaces;

public interface IAlertRepository
{
    Task<IEnumerable<AlertDto>> GetAllAsync();
    Task<AlertDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<AlertDto>> GetActiveByCondominiumAsync(Guid condominiumId);
    Task<Guid> CreateAsync(CreateAlertDto dto, Guid userId, Guid condominiumId);
    Task UpdateAsync(Guid id, UpdateAlertDto dto);
    Task UpdateStatusAsync(Guid id, int status);
    Task SoftDeleteAsync(Guid id);
}
