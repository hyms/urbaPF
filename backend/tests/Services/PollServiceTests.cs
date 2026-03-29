using FluentAssertions;
using Moq;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;
using UrbaPF.Infrastructure.Services;

namespace UrbaPF.Tests.Services;

public class PollServiceTests
{
    private readonly Mock<IPollRepository> _pollRepositoryMock;
    private readonly Mock<IVoteRepository> _voteRepositoryMock;
    private readonly PollService _pollService;

    private static readonly Guid CondominiumId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid PollId = Guid.NewGuid();

    private const int RoleAdministrator = 4;
    private const int RoleManager = 3;
    private const int RoleNeighbor = 2;

    private const int PollStatusDraft = 1;
    private const int PollStatusScheduled = 2;
    private const int PollStatusActive = 3;
    private const int PollStatusClosed = 4;

    public PollServiceTests()
    {
        _pollRepositoryMock = new Mock<IPollRepository>();
        _voteRepositoryMock = new Mock<IVoteRepository>();
        _pollService = new PollService(_pollRepositoryMock.Object, _voteRepositoryMock.Object);
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
            MinRoleToVote = RoleNeighbor
        };

        _pollRepositoryMock
            .Setup(r => r.CreateAsync(dto, UserId, CondominiumId, PollStatusScheduled))
            .ReturnsAsync(PollId);

        var result = await _pollService.CreateAsync(dto, UserId, CondominiumId, RoleManager);

        result.Should().NotBeNull();
        result!.Value.pollId.Should().Be(PollId);
        result.Value.status.Should().Be(PollStatusScheduled);
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
            MinRoleToVote = RoleNeighbor
        };

        _pollRepositoryMock
            .Setup(r => r.CreateAsync(dto, UserId, CondominiumId, PollStatusDraft))
            .ReturnsAsync(PollId);

        var result = await _pollService.CreateAsync(dto, UserId, CondominiumId, RoleNeighbor);

        result.Should().NotBeNull();
        result!.Value.status.Should().Be(PollStatusDraft);
    }

    [Test]
    public async Task UpdateAsync_ActivePoll_ShouldReturnError()
    {
        var poll = new PollDto
        {
            Id = PollId,
            CondominiumId = CondominiumId,
            Title = "Test Poll",
            Status = PollStatusActive,
            ServerSecret = "secret123"
        };

        _pollRepositoryMock.Setup(r => r.GetByIdAsync(PollId)).ReturnsAsync(poll);

        var result = await _pollService.UpdateAsync(PollId, new UpdatePollDto { Title = "New Title" }, RoleManager);

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
            Status = PollStatusClosed,
            ServerSecret = "secret123"
        };

        _pollRepositoryMock.Setup(r => r.GetByIdAsync(PollId)).ReturnsAsync(poll);

        var result = await _pollService.UpdateAsync(PollId, new UpdatePollDto { Title = "New Title" }, RoleManager);

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
            Status = PollStatusDraft,
            ServerSecret = "secret123"
        };

        _pollRepositoryMock.Setup(r => r.GetByIdAsync(PollId)).ReturnsAsync(poll);
        _voteRepositoryMock.Setup(r => r.HasAnyVotesAsync(PollId)).ReturnsAsync(true);

        var result = await _pollService.DeleteAsync(PollId, RoleManager);

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
            Status = PollStatusDraft,
            StartsAt = DateTime.UtcNow.AddDays(1),
            EndsAt = DateTime.UtcNow.AddDays(7),
            MinRoleToVote = RoleNeighbor,
            ServerSecret = "secret123"
        };

        _pollRepositoryMock.Setup(r => r.GetByIdAsync(PollId)).ReturnsAsync(poll);

        var result = await _pollService.VoteAsync(PollId, UserId, RoleNeighbor, 0, "127.0.0.1");

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
            Status = PollStatusActive,
            StartsAt = DateTime.UtcNow.AddDays(-1),
            EndsAt = DateTime.UtcNow.AddDays(7),
            MinRoleToVote = RoleNeighbor,
            ServerSecret = "secret123"
        };

        _pollRepositoryMock.Setup(r => r.GetByIdAsync(PollId)).ReturnsAsync(poll);
        _voteRepositoryMock.Setup(r => r.HasUserVotedAsync(PollId, UserId)).ReturnsAsync(true);

        var result = await _pollService.VoteAsync(PollId, UserId, RoleNeighbor, 0, "127.0.0.1");

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
            Status = PollStatusActive,
            StartsAt = DateTime.UtcNow.AddDays(-1),
            EndsAt = DateTime.UtcNow.AddDays(7),
            MinRoleToVote = RoleNeighbor,
            ServerSecret = "secret123"
        };

        _pollRepositoryMock.Setup(r => r.GetByIdAsync(PollId)).ReturnsAsync(poll);
        _voteRepositoryMock.Setup(r => r.HasUserVotedAsync(PollId, UserId)).ReturnsAsync(false);
        _voteRepositoryMock.Setup(r => r.CreateAsync(UserId, PollId, 0, It.IsAny<string>(), "127.0.0.1"))
            .ReturnsAsync(Guid.NewGuid());

        var result = await _pollService.VoteAsync(PollId, UserId, RoleNeighbor, 0, "127.0.0.1");

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
            Status = PollStatusActive,
            StartsAt = DateTime.UtcNow.AddDays(-1),
            EndsAt = DateTime.UtcNow.AddDays(7),
            MinRoleToVote = RoleManager,
            ServerSecret = "secret123"
        };

        _pollRepositoryMock.Setup(r => r.GetByIdAsync(PollId)).ReturnsAsync(poll);

        var result = await _pollService.VoteAsync(PollId, UserId, RoleNeighbor, 0, "127.0.0.1");

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
            Status = PollStatusDraft,
            ServerSecret = "secret123"
        };

        _pollRepositoryMock.Setup(r => r.GetByIdAsync(PollId)).ReturnsAsync(poll);

        var result = await _pollService.DeleteAsync(PollId, RoleNeighbor);

        result.success.Should().BeFalse();
        result.error.Should().Contain("No tienes permiso");
    }
}
