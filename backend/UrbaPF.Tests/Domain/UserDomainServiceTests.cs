using FluentAssertions;
using UrbaPF.Domain.Entities;
using UrbaPF.Domain.Services;
namespace UrbaPF.Tests.Domain;
public class UserDomainServiceTests {
    private readonly UserDomainService _service = new();
    [Test] public void CanUserVote_WithSufficientRole_ReturnsTrue() { var user = new User { Role = 2, Status = 1 }; _service.CanUserVote(user, 2).Should().BeTrue(); }
    [Test] public void IsUserActive_WithActiveStatus_ReturnsTrue() { var user = new User { Status = 1 }; _service.IsUserActive(user).Should().BeTrue(); }
}
