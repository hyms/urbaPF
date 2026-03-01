using UrbaPF.Domain.Entities;
using UrbaPF.Domain.Services.Interfaces;

namespace UrbaPF.Domain.Services;

public class UserDomainService : IUserDomainService
{
    public bool CanUserVote(User user, int requiredRole)
    {
        return user.Role >= requiredRole && user.Status == 1;
    }

    public bool CanUserCreateIncident(User user)
    {
        return user.Status == 1;
    }

    public bool CanUserCreatePost(User user)
    {
        return user.Status == 1 && user.IsValidated;
    }

    public bool CanUserBeValidated(int managerVotes)
    {
        return managerVotes >= 2;
    }

    public int CalculateCredibilityLevel(int managerVotes)
    {
        return managerVotes switch
        {
            >= 5 => 5,
            >= 3 => 4,
            >= 2 => 3,
            >= 1 => 2,
            _ => 1
        };
    }

    public bool IsUserActive(User user)
    {
        return user.Status == 1;
    }
}

public class IncidentDomainService : IIncidentDomainService
{
    public bool CanUpdateIncidentStatus(User user, int currentStatus, int newStatus)
    {
        if (user.Role >= 3) return true;
        if (user.Role >= 2 && newStatus == 4) return true;
        return false;
    }

    public bool IsPriorityHigh(int priority)
    {
        return priority >= 4;
    }

    public int CalculatePriorityScore(int priority, DateTime occurredAt)
    {
        var hoursSinceOccurred = (DateTime.UtcNow - occurredAt).TotalHours;
        return priority + (hoursSinceOccurred > 24 ? 2 : 0);
    }
}

public class PollDomainService : IPollDomainService
{
    public bool CanUserVote(User user, Poll poll)
    {
        if (poll.Status != 1) return false;
        if (user.Role < poll.MinRoleToVote) return false;
        if (user.Status != 1) return false;
        if (IsPollExpired(poll)) return false;
        return true;
    }

    public bool IsPollActive(Poll poll)
    {
        return poll.Status == 1 && !IsPollExpired(poll);
    }

    public bool IsPollExpired(Poll poll)
    {
        return DateTime.UtcNow > poll.EndsAt;
    }

    public string GenerateVoteSignature(Guid pollId, Guid userId, int optionIndex, string secret)
    {
        var data = $"{pollId}:{userId}:{optionIndex}:{secret}";
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }

    public bool ValidateVoteSignature(string signature, Guid pollId, Guid userId, int optionIndex, string secret)
    {
        var expectedSignature = GenerateVoteSignature(pollId, userId, optionIndex, secret);
        return signature == expectedSignature;
    }
}

public class AlertDomainService : IAlertDomainService
{
    public bool IsAlertActive(int status)
    {
        return status is 1 or 2;
    }

    public bool CanUserAcknowledgeAlert(User user)
    {
        return user.Role >= 2 && user.Status == 1;
    }

    public bool IsAlertExpired(Alert alert)
    {
        return DateTime.UtcNow > alert.EstimatedArrival.AddMinutes(30);
    }
}

public class CommentDomainService : ICommentDomainService
{
    public bool CanUserEditComment(User author, User currentUser)
    {
        return author.Id == currentUser.Id || currentUser.Role >= 3;
    }

    public bool CanUserHideComment(User user, int userRole)
    {
        return userRole >= 2;
    }

    public int CalculateCredibilityScore(int credibilityLevel, DateTime commentDate)
    {
        var daysOld = (DateTime.UtcNow - commentDate).TotalDays;
        var decayFactor = Math.Max(0.5, 1 - (daysOld / 365));
        return (int)(credibilityLevel * decayFactor);
    }
}
