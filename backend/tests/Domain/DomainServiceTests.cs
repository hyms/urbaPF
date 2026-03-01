using FluentAssertions;
using UrbaPF.Domain.Entities;
using UrbaPF.Domain.Services;
using UrbaPF.Domain.Services.Interfaces;

namespace UrbaPF.Tests.Domain;

public class UserDomainServiceTests
{
    private readonly UserDomainService _service = new();

    [Fact]
    public void CanUserVote_WithSufficientRole_ReturnsTrue()
    {
        var user = new User { Role = 2, Status = 1 };
        
        var result = _service.CanUserVote(user, 2);
        
        result.Should().BeTrue();
    }

    [Fact]
    public void CanUserVote_WithInsufficientRole_ReturnsFalse()
    {
        var user = new User { Role = 1, Status = 1 };
        
        var result = _service.CanUserVote(user, 2);
        
        result.Should().BeFalse();
    }

    [Fact]
    public void CanUserVote_WithInactiveUser_ReturnsFalse()
    {
        var user = new User { Role = 2, Status = 0 };
        
        var result = _service.CanUserVote(user, 1);
        
        result.Should().BeFalse();
    }

    [Fact]
    public void CanUserCreateIncident_WithActiveUser_ReturnsTrue()
    {
        var user = new User { Status = 1 };
        
        var result = _service.CanUserCreateIncident(user);
        
        result.Should().BeTrue();
    }

    [Fact]
    public void CanUserCreateIncident_WithInactiveUser_ReturnsFalse()
    {
        var user = new User { Status = 0 };
        
        var result = _service.CanUserCreateIncident(user);
        
        result.Should().BeFalse();
    }

    [Fact]
    public void CanUserCreatePost_WithValidatedUser_ReturnsTrue()
    {
        var user = new User { Status = 1, IsValidated = true };
        
        var result = _service.CanUserCreatePost(user);
        
        result.Should().BeTrue();
    }

    [Fact]
    public void CanUserCreatePost_WithNonValidatedUser_ReturnsFalse()
    {
        var user = new User { Status = 1, IsValidated = false };
        
        var result = _service.CanUserCreatePost(user);
        
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(2, true)]
    [InlineData(1, false)]
    public void CanUserBeValidated_WithManagerVotes_ReturnsExpected(int votes, bool expected)
    {
        var result = _service.CanUserBeValidated(votes);
        
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(5, 5)]
    [InlineData(4, 5)]
    [InlineData(3, 4)]
    [InlineData(2, 3)]
    [InlineData(0, 1)]
    public void CalculateCredibilityLevel_ReturnsExpectedLevel(int votes, int expectedLevel)
    {
        var result = _service.CalculateCredibilityLevel(votes);
        
        result.Should().Be(expectedLevel);
    }

    [Fact]
    public void IsUserActive_WithActiveStatus_ReturnsTrue()
    {
        var user = new User { Status = 1 };
        
        var result = _service.IsUserActive(user);
        
        result.Should().BeTrue();
    }

    [Fact]
    public void IsUserActive_WithInactiveStatus_ReturnsFalse()
    {
        var user = new User { Status = 0 };
        
        var result = _service.IsUserActive(user);
        
        result.Should().BeFalse();
    }
}

public class IncidentDomainServiceTests
{
    private readonly IncidentDomainService _service = new();

    [Fact]
    public void CanUpdateIncidentStatus_AsAdmin_ReturnsTrue()
    {
        var user = new User { Role = 4 };
        
        var result = _service.CanUpdateIncidentStatus(user, 1, 2);
        
        result.Should().BeTrue();
    }

    [Fact]
    public void CanUpdateIncidentStatus_AsManager_Resolve_ReturnsTrue()
    {
        var user = new User { Role = 2 };
        
        var result = _service.CanUpdateIncidentStatus(user, 1, 4);
        
        result.Should().BeTrue();
    }

    [Fact]
    public void CanUpdateIncidentStatus_AsNeighbor_CannotResolve_ReturnsFalse()
    {
        var user = new User { Role = 1 };
        
        var result = _service.CanUpdateIncidentStatus(user, 1, 4);
        
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(4, true)]
    [InlineData(5, true)]
    [InlineData(3, false)]
    [InlineData(1, false)]
    public void IsPriorityHigh_ReturnsExpected(int priority, bool expected)
    {
        var result = _service.IsPriorityHigh(priority);
        
        result.Should().Be(expected);
    }

    [Fact]
    public void CalculatePriorityScore_RecentIncident_ReturnsBaseScore()
    {
        var result = _service.CalculatePriorityScore(3, DateTime.UtcNow);
        
        result.Should().Be(3);
    }

    [Fact]
    public void CalculatePriorityScore_OldIncident_ReturnsHigherScore()
    {
        var oldDate = DateTime.UtcNow.AddHours(-48);
        var result = _service.CalculatePriorityScore(3, oldDate);
        
        result.Should().Be(5);
    }
}

public class PollDomainServiceTests
{
    private readonly PollDomainService _service = new();

    [Fact]
    public void CanUserVote_WithValidPoll_ReturnsTrue()
    {
        var user = new User { Role = 2, Status = 1 };
        var poll = new Poll { Status = 1, MinRoleToVote = 1, StartsAt = DateTime.UtcNow.AddDays(-1), EndsAt = DateTime.UtcNow.AddDays(1) };
        
        var result = _service.CanUserVote(user, poll);
        
        result.Should().BeTrue();
    }

    [Fact]
    public void CanUserVote_WithInactivePoll_ReturnsFalse()
    {
        var user = new User { Role = 2, Status = 1 };
        var poll = new Poll { Status = 0 };
        
        var result = _service.CanUserVote(user, poll);
        
        result.Should().BeFalse();
    }

    [Fact]
    public void CanUserVote_WithExpiredPoll_ReturnsFalse()
    {
        var user = new User { Role = 2, Status = 1 };
        var poll = new Poll { Status = 1, StartsAt = DateTime.UtcNow.AddDays(-2), EndsAt = DateTime.UtcNow.AddDays(-1) };
        
        var result = _service.CanUserVote(user, poll);
        
        result.Should().BeFalse();
    }

    [Fact]
    public void IsPollActive_WithActiveStatus_ReturnsTrue()
    {
        var poll = new Poll { Status = 1, StartsAt = DateTime.UtcNow.AddDays(-1), EndsAt = DateTime.UtcNow.AddDays(1) };
        
        var result = _service.IsPollActive(poll);
        
        result.Should().BeTrue();
    }

    [Fact]
    public void IsPollExpired_AfterEndDate_ReturnsTrue()
    {
        var poll = new Poll { Status = 1, StartsAt = DateTime.UtcNow.AddDays(-2), EndsAt = DateTime.UtcNow.AddDays(-1) };
        
        var result = _service.IsPollExpired(poll);
        
        result.Should().BeTrue();
    }

    [Fact]
    public void GenerateVoteSignature_ReturnsConsistentSignature()
    {
        var sig1 = _service.GenerateVoteSignature(Guid.NewGuid(), Guid.NewGuid(), 1, "secret");
        var sig2 = _service.GenerateVoteSignature(Guid.NewGuid(), Guid.NewGuid(), 1, "secret");
        
        sig1.Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateVoteSignature_WithValidSignature_ReturnsTrue()
    {
        var pollId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var secret = "secret";
        var signature = _service.GenerateVoteSignature(pollId, userId, 1, secret);
        
        var result = _service.ValidateVoteSignature(signature, pollId, userId, 1, secret);
        
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateVoteSignature_WithInvalidSignature_ReturnsFalse()
    {
        var result = _service.ValidateVoteSignature("invalid", Guid.NewGuid(), Guid.NewGuid(), 1, "secret");
        
        result.Should().BeFalse();
    }
}

public class AlertDomainServiceTests
{
    private readonly AlertDomainService _service = new();

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, false)]
    [InlineData(4, false)]
    public void IsAlertActive_ReturnsExpected(int status, bool expected)
    {
        var result = _service.IsAlertActive(status);
        
        result.Should().Be(expected);
    }

    [Fact]
    public void CanUserAcknowledgeAlert_AsManager_ReturnsTrue()
    {
        var user = new User { Role = 2, Status = 1 };
        
        var result = _service.CanUserAcknowledgeAlert(user);
        
        result.Should().BeTrue();
    }

    [Fact]
    public void CanUserAcknowledgeAlert_AsNeighbor_ReturnsFalse()
    {
        var user = new User { Role = 1, Status = 1 };
        
        var result = _service.CanUserAcknowledgeAlert(user);
        
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAlertExpired_AfterTimeout_ReturnsTrue()
    {
        var alert = new Alert { EstimatedArrival = DateTime.UtcNow.AddHours(-1) };
        
        var result = _service.IsAlertExpired(alert);
        
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAlertExpired_BeforeTimeout_ReturnsFalse()
    {
        var alert = new Alert { EstimatedArrival = DateTime.UtcNow.AddMinutes(10) };
        
        var result = _service.IsAlertExpired(alert);
        
        result.Should().BeFalse();
    }
}

public class CommentDomainServiceTests
{
    private readonly CommentDomainService _service = new();

    [Fact]
    public void CanUserEditComment_AsAuthor_ReturnsTrue()
    {
        var author = new User { Id = Guid.NewGuid() };
        var currentUser = new User { Id = author.Id, Role = 1 };
        
        var result = _service.CanUserEditComment(author, currentUser);
        
        result.Should().BeTrue();
    }

    [Fact]
    public void CanUserEditComment_AsAdmin_ReturnsTrue()
    {
        var author = new User { Id = Guid.NewGuid() };
        var admin = new User { Id = Guid.NewGuid(), Role = 4 };
        
        var result = _service.CanUserEditComment(author, admin);
        
        result.Should().BeTrue();
    }

    [Fact]
    public void CanUserHideComment_AsManager_ReturnsTrue()
    {
        var result = _service.CanUserHideComment(new User(), 2);
        
        result.Should().BeTrue();
    }

    [Fact]
    public void CanUserHideComment_AsNeighbor_ReturnsFalse()
    {
        var result = _service.CanUserHideComment(new User(), 1);
        
        result.Should().BeFalse();
    }

    [Fact]
    public void CalculateCredibilityScore_NewComment_ReturnsFullScore()
    {
        var result = _service.CalculateCredibilityScore(5, DateTime.UtcNow.AddMinutes(-1));
        
        result.Should().BeGreaterOrEqualTo(4);
    }

    [Fact]
    public void CalculateCredibilityScore_OldComment_ReturnsDecayedScore()
    {
        var oldDate = DateTime.UtcNow.AddDays(-200);
        var result = _service.CalculateCredibilityScore(5, oldDate);
        
        result.Should().BeLessOrEqualTo(3);
    }
}
