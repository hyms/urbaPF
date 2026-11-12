using UrbaPF.Application.DTOs;

namespace UrbaPF.Application.Interfaces;

public interface ICondominiumRepository
{
    Task<IEnumerable<CondominiumDto>> GetAllAsync();
    Task<CondominiumDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(CreateCondominiumDto dto);
    Task UpdateAsync(Guid id, UpdateCondominiumDto dto);
    Task SoftDeleteAsync(Guid id);
}
