using UrbaPF.Domain.Entities;
namespace UrbaPF.Domain.Services.Interfaces;
public interface IPollDomainService {
    bool CanUserVote(User user, Poll poll);
    bool IsPollActive(Poll poll);
    bool IsPollExpired(Poll poll);
    string GenerateVoteSignature(Guid pollId, Guid userId, int optionIndex, string secret);
    bool ValidateVoteSignature(string signature, Guid pollId, Guid userId, int optionIndex, string secret);
}
