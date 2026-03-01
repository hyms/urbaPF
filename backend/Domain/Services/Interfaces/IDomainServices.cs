using UrbaPF.Domain.Entities;

namespace UrbaPF.Domain.Services.Interfaces;

public interface IUserDomainService
{
    bool CanUserVote(User user, int requiredRole);
    bool CanUserCreateIncident(User user);
    bool CanUserCreatePost(User user);
    bool CanUserBeValidated(int managerVotes);
    int CalculateCredibilityLevel(int managerVotes);
    bool IsUserActive(User user);
}

public interface IIncidentDomainService
{
    bool CanUpdateIncidentStatus(User user, int currentStatus, int newStatus);
    bool IsPriorityHigh(int priority);
    int CalculatePriorityScore(int priority, DateTime occurredAt);
}

public interface IPollDomainService
{
    bool CanUserVote(User user, Poll poll);
    bool IsPollActive(Poll poll);
    bool IsPollExpired(Poll poll);
    string GenerateVoteSignature(Guid pollId, Guid userId, int optionIndex, string secret);
    bool ValidateVoteSignature(string signature, Guid pollId, Guid userId, int optionIndex, string secret);
}

public interface IAlertDomainService
{
    bool IsAlertActive(int status);
    bool CanUserAcknowledgeAlert(User user);
    bool IsAlertExpired(Alert alert);
}

public interface ICommentDomainService
{
    bool CanUserEditComment(User author, User currentUser);
    bool CanUserHideComment(User user, int userRole);
    int CalculateCredibilityScore(int credibilityLevel, DateTime commentDate);
}
