using FluentAssertions;
using UrbaPF.Domain.Entities;
using UrbaPF.Domain.Services;
namespace UrbaPF.Tests.Domain;
public class PollDomainServiceTests {
    private readonly PollDomainService _service = new();
    [Test] public void IsPollActive_WithActiveStatus_ReturnsTrue() { var poll = new Poll { Status = 1, StartsAt = DateTime.UtcNow.AddDays(-1), EndsAt = DateTime.UtcNow.AddDays(1) }; _service.IsPollActive(poll).Should().BeTrue(); }
}
