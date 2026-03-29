using UrbaPF.Infrastructure.DTOs;

namespace UrbaPF.Infrastructure.Interfaces;

public interface IVoteRepository
{
    Task<IEnumerable<VoteDto>> GetByPollAsync(Guid pollId);
    Task<VoteDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(Guid userId, Guid pollId, int optionIndex, string signature, string ipAddress);
    Task<bool> HasUserVotedAsync(Guid pollId, Guid userId);
    Task<bool> HasAnyVotesAsync(Guid pollId);
    Task<Dictionary<int, int>> GetResultsAsync(Guid pollId);
}
