using UrbaPF.Infrastructure.DTOs;

namespace UrbaPF.Infrastructure.Interfaces;

public interface IPollRepository
{
    Task<IEnumerable<PollDto>> GetAllAsync();
    Task<PollDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<PollDto>> GetByCondominiumAsync(Guid condominiumId);
    Task<Guid> CreateAsync(CreatePollDto dto, Guid userId, Guid condominiumId, int status = 0);
    Task UpdateAsync(Guid id, UpdatePollDto dto);
    Task SoftDeleteAsync(Guid id);
}
