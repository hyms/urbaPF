using FluentAssertions;
using Moq;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;
using UrbaPF.Infrastructure.Services;
using UrbaPF.Domain.Enums;

namespace UrbaPF.Tests.Services;

public class PollServiceTests
{
    private readonly Mock<IPollRepository> _pollRepositoryMock;
    private readonly Mock<IVoteRepository> _voteRepositoryMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly PollService _pollService;

    private static readonly Guid CondominiumId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid PollId = Guid.NewGuid();


    public PollServiceTests()
    {
        _pollRepositoryMock = new Mock<IPollRepository>();
        _voteRepositoryMock = new Mock<IVoteRepository>();
        _auditServiceMock = new Mock<IAuditService>();
        _pollService = new PollService(
            _pollRepositoryMock.Object, 
            _voteRepositoryMock.Object,
            _auditServiceMock.Object);
    }


    [Test]
    public async Task CreateAsync_ManagerRole_ShouldSetScheduledStatus()
    {
        var dto = new CreatePollDto
        {
            Title = "Test Poll",
            Description = "Test Description",
            Options = "[\"Option1\", \"Option2\"]",
            PollType = 1,
            StartsAt = DateTime.UtcNow.AddDays(1),
            EndsAt = DateTime.UtcNow.AddDays(7),
            RequiresJustification = false,
            MinRoleToVote = (int)UserRole.Neighbor
        };

        _pollRepositoryMock
            .Setup(r => r.CreateAsync(dto, UserId, CondominiumId, (int)PollStatus.Scheduled))
            .ReturnsAsync(PollId);

        var result = await _pollService.CreateAsync(dto, UserId, CondominiumId, UserRole.Manager);

        result.Should().NotBeNull();
        result!.Value.pollId.Should().Be(PollId);
        result.Value.status.Should().Be((int)PollStatus.Scheduled);
        _auditServiceMock.Verify(a => a.LogEventAsync(UserId, CondominiumId, "POLL_CREATED", PollId, dto), Times.Once);
    }

    [Test]
    public async Task CreateAsync_NeighborRole_ShouldSetDraftStatus()
    {
        var dto = new CreatePollDto
        {
            Title = "Test Poll",
            Description = "Test Description",
            Options = "[\"Option1\", \"Option2\"]",
            PollType = 1,
            StartsAt = DateTime.UtcNow.AddDays(1),
            EndsAt = DateTime.UtcNow.AddDays(7),
            RequiresJustification = false,
            MinRoleToVote = (int)UserRole.Neighbor
        };

        _pollRepositoryMock
            .Setup(r => r.CreateAsync(dto, UserId, CondominiumId, (int)PollStatus.Draft))
            .ReturnsAsync(PollId);

        var result = await _pollService.CreateAsync(dto, UserId, CondominiumId, UserRole.Neighbor);

        result.Should().NotBeNull();
        result!.Value.status.Should().Be((int)PollStatus.Draft);
        _auditServiceMock.Verify(a => a.LogEventAsync(UserId, CondominiumId, "POLL_CREATED", PollId, dto), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_ActivePoll_ShouldReturnError()
    {
        var poll = new PollDto
        {
            Id = PollId,
            CondominiumId = CondominiumId,
            Title = "Test Poll",
            Status = (int)PollStatus.Active,
            ServerSecret = "secret123"
        };

        _pollRepositoryMock.Setup(r => r.GetByIdAsync(PollId)).ReturnsAsync(poll);

        var result = await _pollService.UpdateAsync(PollId, new UpdatePollDto { Title = "New Title" }, UserRole.Manager);

        result.success.Should().BeFalse();
        result.error.Should().Contain("activa o cerrada");
    }

    [Test]
    public async Task UpdateAsync_ClosedPoll_ShouldReturnError()
    {
        var poll = new PollDto
        {
            Id = PollId,
            CondominiumId = CondominiumId,
            Title = "Test Poll",
            Status = (int)PollStatus.Closed,
            ServerSecret = "secret123"
        };

        _pollRepositoryMock.Setup(r => r.GetByIdAsync(PollId)).ReturnsAsync(poll);

        var result = await _pollService.UpdateAsync(PollId, new UpdatePollDto { Title = "New Title" }, UserRole.Manager);

        result.success.Should().BeFalse();
        result.error.Should().Contain("activa o cerrada");
    }

    [Test]
    public async Task DeleteAsync_PollWithVotes_ShouldReturnError()
    {
        var poll = new PollDto
        {
            Id = PollId,
            CondominiumId = CondominiumId,
            Title = "Test Poll",
            Status = (int)PollStatus.Draft,
            ServerSecret = "secret123"
        };

        _pollRepositoryMock.Setup(r => r.GetByIdAsync(PollId)).ReturnsAsync(poll);
        _voteRepositoryMock.Setup(r => r.HasAnyVotesAsync(PollId)).ReturnsAsync(true);

        var result = await _pollService.DeleteAsync(PollId, UserRole.Manager);

        result.success.Should().BeFalse();
        result.error.Should().Contain("ya tiene votos");
    }

    [Test]
    public async Task VoteAsync_PollNotActive_ShouldReturnError()
    {
        var poll = new PollDto
        {
            Id = PollId,
            CondominiumId = CondominiumId,
            Title = "Test Poll",
            Status = (int)PollStatus.Draft,
            StartsAt = DateTime.UtcNow.AddDays(1),
            EndsAt = DateTime.UtcNow.AddDays(7),
            MinRoleToVote = (int)UserRole.Neighbor,
            ServerSecret = "secret123"
        };

        _pollRepositoryMock.Setup(r => r.GetByIdAsync(PollId)).ReturnsAsync(poll);

        var result = await _pollService.VoteAsync(PollId, UserId, UserRole.Neighbor, 0, "127.0.0.1");

        result.success.Should().BeFalse();
        result.error.Should().Contain("no está activa");
    }

    [Test]
    public async Task VoteAsync_UserAlreadyVoted_ShouldReturnError()
    {
        var poll = new PollDto
        {
            Id = PollId,
            CondominiumId = CondominiumId,
            Title = "Test Poll",
            Status = (int)PollStatus.Active,
            StartsAt = DateTime.UtcNow.AddDays(-1),
            EndsAt = DateTime.UtcNow.AddDays(7),
            MinRoleToVote = (int)UserRole.Neighbor,
            ServerSecret = "secret123"
        };

        _pollRepositoryMock.Setup(r => r.GetByIdAsync(PollId)).ReturnsAsync(poll);
        _voteRepositoryMock.Setup(r => r.HasUserVotedAsync(PollId, UserId)).ReturnsAsync(true);

        var result = await _pollService.VoteAsync(PollId, UserId, UserRole.Neighbor, 0, "127.0.0.1");

        result.success.Should().BeFalse();
        result.error.Should().Contain("Ya has votado");
    }

    [Test]
    public async Task VoteAsync_ValidVote_ShouldSucceed()
    {
        var poll = new PollDto
        {
            Id = PollId,
            CondominiumId = CondominiumId,
            Title = "Test Poll",
            Status = (int)PollStatus.Active,
            StartsAt = DateTime.UtcNow.AddDays(-1),
            EndsAt = DateTime.UtcNow.AddDays(7),
            MinRoleToVote = (int)UserRole.Neighbor,
            ServerSecret = "secret123"
        };

        _pollRepositoryMock.Setup(r => r.GetByIdAsync(PollId)).ReturnsAsync(poll);
        _voteRepositoryMock.Setup(r => r.HasUserVotedAsync(PollId, UserId)).ReturnsAsync(false);
        _voteRepositoryMock.Setup(r => r.CreateAsync(UserId, PollId, 0, It.IsAny<string>(), "127.0.0.1"))
            .ReturnsAsync(Guid.NewGuid());

        var result = await _pollService.VoteAsync(PollId, UserId, UserRole.Neighbor, 0, "127.0.0.1");

        result.success.Should().BeTrue();
        result.error.Should().BeNull();
        _voteRepositoryMock.Verify(r => r.CreateAsync(UserId, PollId, 0, It.IsAny<string>(), "127.0.0.1"), Times.Once);
    }

    [Test]
    public async Task VoteAsync_InsufficientRole_ShouldReturnError()
    {
        var poll = new PollDto
        {
            Id = PollId,
            CondominiumId = CondominiumId,
            Title = "Test Poll",
            Status = (int)PollStatus.Active,
            StartsAt = DateTime.UtcNow.AddDays(-1),
            EndsAt = DateTime.UtcNow.AddDays(7),
            MinRoleToVote = (int)UserRole.Manager,
            ServerSecret = "secret123"
        };

        _pollRepositoryMock.Setup(r => r.GetByIdAsync(PollId)).ReturnsAsync(poll);

        var result = await _pollService.VoteAsync(PollId, UserId, UserRole.Neighbor, 0, "127.0.0.1");

        result.success.Should().BeFalse();
        result.error.Should().Contain("rol mínimo");
    }

    [Test]
    public async Task DeleteAsync_NonManagerRole_ShouldReturnError()
    {
        var poll = new PollDto
        {
            Id = PollId,
            CondominiumId = CondominiumId,
            Title = "Test Poll",
            Status = (int)PollStatus.Draft,
            ServerSecret = "secret123"
        };

        _pollRepositoryMock.Setup(r => r.GetByIdAsync(PollId)).ReturnsAsync(poll);

        var result = await _pollService.DeleteAsync(PollId, UserRole.Neighbor);

        result.success.Should().BeFalse();
        result.error.Should().Contain("No tienes permiso");
    }
}
