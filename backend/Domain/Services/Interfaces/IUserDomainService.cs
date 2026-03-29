using UrbaPF.Domain.Entities;
namespace UrbaPF.Domain.Services.Interfaces;
public interface IUserDomainService {
    bool CanUserVote(User user, int requiredRole);
    bool CanUserCreateIncident(User user);
    bool CanUserCreatePost(User user);
    bool CanUserBeValidated(int managerVotes);
    int CalculateCredibilityLevel(int managerVotes);
    bool IsUserActive(User user);
}
