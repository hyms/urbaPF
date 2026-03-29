using UrbaPF.Domain.Entities;
using UrbaPF.Domain.Services.Interfaces;
namespace UrbaPF.Domain.Services;
public class CommentDomainService : ICommentDomainService {
    public bool CanUserEditComment(User author, User currentUser) => author.Id == currentUser.Id || currentUser.Role >= 3;
    public bool CanUserHideComment(User user, int userRole) => userRole >= 2;
    public int CalculateCredibilityScore(int credibilityLevel, DateTime commentDate) {
        var daysOld = (DateTime.UtcNow - commentDate).TotalDays;
        var decayFactor = Math.Max(0.5, 1 - (daysOld / 365));
        return (int)(credibilityLevel * decayFactor);
    }
}
