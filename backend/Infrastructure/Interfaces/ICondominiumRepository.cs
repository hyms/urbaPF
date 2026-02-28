using UrbaPF.Infrastructure.DTOs;

namespace UrbaPF.Infrastructure.Interfaces;

public interface ICondominiumRepository
{
    Task<IEnumerable<CondominiumDto>> GetAllAsync();
    Task<CondominiumDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(CreateCondominiumDto dto);
    Task UpdateAsync(Guid id, UpdateCondominiumDto dto);
    Task SoftDeleteAsync(Guid id);
}
