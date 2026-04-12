using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Domain.Enums;

namespace UrbaPF.Infrastructure.Interfaces;

public interface IPollService
{
    Task<PollDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<PollDto>> GetByCondominiumAsync(Guid condominiumId);
    Task<(Guid pollId, int status)?> CreateAsync(CreatePollDto dto, Guid userId, Guid condominiumId, UserRole userRole);
    Task<(bool success, string? error)> UpdateAsync(Guid id, UpdatePollDto dto, UserRole userRole);
    Task<(bool success, string? error)> DeleteAsync(Guid id, UserRole userRole);
    Task<(bool success, string? error)> VoteAsync(Guid pollId, Guid userId, UserRole userRole, int optionIndex, string ipAddress);
    Task<VoteResultDto?> GetResultsAsync(Guid pollId);
    Task<(bool isValid, string? error)> VerifyVoteAsync(Guid pollId, Guid userId, int optionIndex, string signature);
}

public class VoteResultDto
{
    public IEnumerable<VoteDto> Votes { get; set; } = [];
    public Dictionary<int, int> Results { get; set; } = new();
}
