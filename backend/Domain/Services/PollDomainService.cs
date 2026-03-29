using UrbaPF.Domain.Entities;
using UrbaPF.Domain.Services.Interfaces;
namespace UrbaPF.Domain.Services;
public class PollDomainService : IPollDomainService {
    public bool CanUserVote(User user, Poll poll) {
        if (poll.Status != 1) return false;
        if (user.Role < poll.MinRoleToVote) return false;
        if (user.Status != 1) return false;
        return !IsPollExpired(poll);
    }
    public bool IsPollActive(Poll poll) => poll.Status == 1 && !IsPollExpired(poll);
    public bool IsPollExpired(Poll poll) => DateTime.UtcNow > poll.EndsAt;
    public string GenerateVoteSignature(Guid pollId, Guid userId, int optionIndex, string secret) {
        var data = $"{pollId}:{userId}:{optionIndex}:{secret}";
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }
    public bool ValidateVoteSignature(string signature, Guid pollId, Guid userId, int optionIndex, string secret) => signature == GenerateVoteSignature(pollId, userId, optionIndex, secret);
}
