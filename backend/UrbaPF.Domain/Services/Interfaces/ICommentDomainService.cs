using UrbaPF.Domain.Entities;
namespace UrbaPF.Domain.Services.Interfaces;
public interface ICommentDomainService {
    bool CanUserEditComment(User author, User currentUser);
    bool CanUserHideComment(User user, int userRole);
    int CalculateCredibilityScore(int credibilityLevel, DateTime commentDate);
}
