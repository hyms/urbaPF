using UrbaPF.Domain.Entities;
using UrbaPF.Domain.Services.Interfaces;
namespace UrbaPF.Domain.Services;
public class UserDomainService : IUserDomainService {
    public bool CanUserVote(User user, int requiredRole) => user.Role >= requiredRole && user.Status == 1;
    public bool CanUserCreateIncident(User user) => user.Status == 1;
    public bool CanUserCreatePost(User user) => user.Status == 1 && user.IsValidated;
    public bool CanUserBeValidated(int managerVotes) => managerVotes >= 2;
    public int CalculateCredibilityLevel(int managerVotes) => managerVotes switch { >= 4 => 5, >= 3 => 4, >= 2 => 3, >= 1 => 2, _ => 1 };
    public bool IsUserActive(User user) => user.Status == 1;
}
