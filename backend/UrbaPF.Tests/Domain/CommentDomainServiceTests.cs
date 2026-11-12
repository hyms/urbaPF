using FluentAssertions;
using UrbaPF.Domain.Entities;
using UrbaPF.Domain.Services;
namespace UrbaPF.Tests.Domain;
public class CommentDomainServiceTests {
    private readonly CommentDomainService _service = new();
    [Test]
    public void CanUserEditComment_AsAuthor_ReturnsTrue() {
        var authorId = Guid.NewGuid();
        var author = new User { Id = authorId };
        var currentUser = new User { Id = authorId, Role = 1 };
        _service.CanUserEditComment(author, currentUser).Should().BeTrue();
    }
    [Test]
    public void CalculateCredibilityScore_NewComment_ReturnsFullScore() {
        var result = _service.CalculateCredibilityScore(5, DateTime.UtcNow.AddMinutes(-1));
        result.Should().BeGreaterThanOrEqualTo(4);
    }
}
