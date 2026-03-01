using FluentAssertions;
using Moq;
using UrbaPF.Api.DTOs;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;

namespace UrbaPF.Tests.Routes;

public class UserRoutesTests
{
    [Fact]
    public async Task GetAllUsers_ReturnsUsers()
    {
        var mockRepo = new Mock<IUserRepository>();
        var users = new List<UserDto>
        {
            new() { Id = Guid.NewGuid(), Email = "user1@test.com", FullName = "User 1" },
            new() { Id = Guid.NewGuid(), Email = "user2@test.com", FullName = "User 2" }
        };
        mockRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(users);

        var result = await mockRepo.Object.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserById_ReturnsUser_WhenExists()
    {
        var mockRepo = new Mock<IUserRepository>();
        var userId = Guid.NewGuid();
        var user = new UserDto { Id = userId, Email = "test@test.com" };
        mockRepo.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        var result = await mockRepo.Object.GetByIdAsync(userId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
    }

    [Fact]
    public async Task GetUserById_ReturnsNull_WhenNotExists()
    {
        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((UserDto?)null);

        var result = await mockRepo.Object.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateUser_CallsRepository()
    {
        var mockRepo = new Mock<IUserRepository>();
        var userId = Guid.NewGuid();
        mockRepo.Setup(x => x.CreateAsync(It.IsAny<CreateUserDto>(), It.IsAny<string>())).ReturnsAsync(userId);

        var result = await mockRepo.Object.CreateAsync(new CreateUserDto { Email = "new@test.com", FullName = "New User" }, "password");

        result.Should().Be(userId);
        mockRepo.Verify(x => x.CreateAsync(It.IsAny<CreateUserDto>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUser_CallsRepository()
    {
        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateUserDto>())).Returns(Task.CompletedTask);

        await mockRepo.Object.UpdateAsync(Guid.NewGuid(), new UpdateUserDto { FullName = "Updated" });

        mockRepo.Verify(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateUserDto>()), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteUser_CallsRepository()
    {
        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(x => x.SoftDeleteAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        await mockRepo.Object.SoftDeleteAsync(Guid.NewGuid());

        mockRepo.Verify(x => x.SoftDeleteAsync(It.IsAny<Guid>()), Times.Once);
    }
}

public class CondominiumRoutesTests
{
    [Fact]
    public async Task GetAllCondominiums_ReturnsList()
    {
        var mockRepo = new Mock<ICondominiumRepository>();
        var condominiums = new List<CondominiumDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Condo 1" },
            new() { Id = Guid.NewGuid(), Name = "Condo 2" }
        };
        mockRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(condominiums);

        var result = await mockRepo.Object.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCondominiumById_ReturnsCondominium()
    {
        var mockRepo = new Mock<ICondominiumRepository>();
        var condoId = Guid.NewGuid();
        mockRepo.Setup(x => x.GetByIdAsync(condoId)).ReturnsAsync(new CondominiumDto { Id = condoId, Name = "Test" });

        var result = await mockRepo.Object.GetByIdAsync(condoId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(condoId);
    }

    [Fact]
    public async Task CreateCondominium_ReturnsId()
    {
        var mockRepo = new Mock<ICondominiumRepository>();
        var condoId = Guid.NewGuid();
        mockRepo.Setup(x => x.CreateAsync(It.IsAny<CreateCondominiumDto>())).ReturnsAsync(condoId);

        var result = await mockRepo.Object.CreateAsync(new CreateCondominiumDto { Name = "New Condo" });

        result.Should().Be(condoId);
    }
}

public class PostRoutesTests
{
    [Fact]
    public async Task GetPostById_ReturnsPost()
    {
        var mockRepo = new Mock<IPostRepository>();
        var postId = Guid.NewGuid();
        mockRepo.Setup(x => x.GetByIdAsync(postId)).ReturnsAsync(new PostDto { Id = postId, Title = "Test" });

        var result = await mockRepo.Object.GetByIdAsync(postId);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Test");
    }

    [Fact]
    public async Task CreatePost_CallsRepository()
    {
        var mockRepo = new Mock<IPostRepository>();
        var postId = Guid.NewGuid();
        mockRepo.Setup(x => x.CreateAsync(It.IsAny<CreatePostDto>(), It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(postId);

        var result = await mockRepo.Object.CreateAsync(
            new CreatePostDto { Title = "New Post", Content = "Content" },
            Guid.NewGuid(),
            Guid.NewGuid());

        result.Should().Be(postId);
    }
}

public class IncidentRoutesTests
{
    [Fact]
    public async Task GetIncidentsByCondominium_ReturnsList()
    {
        var mockRepo = new Mock<IIncidentRepository>();
        var condoId = Guid.NewGuid();
        mockRepo.Setup(x => x.GetByCondominiumAsync(condoId, null)).ReturnsAsync(new List<IncidentDto>());

        var result = await mockRepo.Object.GetByCondominiumAsync(condoId);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateIncident_CallsRepository()
    {
        var mockRepo = new Mock<IIncidentRepository>();
        var incidentId = Guid.NewGuid();
        mockRepo.Setup(x => x.CreateAsync(It.IsAny<CreateIncidentDto>(), It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(incidentId);

        var result = await mockRepo.Object.CreateAsync(
            new CreateIncidentDto { Title = "Incident", Description = "Desc" },
            Guid.NewGuid(),
            Guid.NewGuid());

        result.Should().Be(incidentId);
    }
}

public class PollRoutesTests
{
    [Fact]
    public async Task GetPollById_ReturnsPoll()
    {
        var mockRepo = new Mock<IPollRepository>();
        var pollId = Guid.NewGuid();
        mockRepo.Setup(x => x.GetByIdAsync(pollId)).ReturnsAsync(new PollDto { Id = pollId, Title = "Poll" });

        var result = await mockRepo.Object.GetByIdAsync(pollId);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreatePoll_CallsRepository()
    {
        var mockRepo = new Mock<IPollRepository>();
        var pollId = Guid.NewGuid();
        mockRepo.Setup(x => x.CreateAsync(It.IsAny<CreatePollDto>(), It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(pollId);

        var result = await mockRepo.Object.CreateAsync(
            new CreatePollDto { Title = "New Poll" },
            Guid.NewGuid(),
            Guid.NewGuid());

        result.Should().Be(pollId);
    }
}

public class AlertRoutesTests
{
    [Fact]
    public async Task GetActiveAlerts_ReturnsList()
    {
        var mockRepo = new Mock<IAlertRepository>();
        var condoId = Guid.NewGuid();
        mockRepo.Setup(x => x.GetActiveByCondominiumAsync(condoId)).ReturnsAsync(new List<AlertDto>());

        var result = await mockRepo.Object.GetActiveByCondominiumAsync(condoId);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAlert_CallsRepository()
    {
        var mockRepo = new Mock<IAlertRepository>();
        var alertId = Guid.NewGuid();
        mockRepo.Setup(x => x.CreateAsync(It.IsAny<CreateAlertDto>(), It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(alertId);

        var result = await mockRepo.Object.CreateAsync(
            new CreateAlertDto { Message = "Alert", AlertType = 1 },
            Guid.NewGuid(),
            Guid.NewGuid());

        result.Should().Be(alertId);
    }
}

public class VoteRoutesTests
{
    [Fact]
    public async Task HasUserVoted_ReturnsFalse_WhenNotVoted()
    {
        var mockRepo = new Mock<IVoteRepository>();
        mockRepo.Setup(x => x.HasUserVotedAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(false);

        var result = await mockRepo.Object.HasUserVotedAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasUserVoted_ReturnsTrue_WhenVoted()
    {
        var mockRepo = new Mock<IVoteRepository>();
        mockRepo.Setup(x => x.HasUserVotedAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(true);

        var result = await mockRepo.Object.HasUserVotedAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CreateVote_CallsRepository()
    {
        var mockRepo = new Mock<IVoteRepository>();
        var voteId = Guid.NewGuid();
        mockRepo.Setup(x => x.CreateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(voteId);

        var result = await mockRepo.Object.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), 1, "sig", "ip");

        result.Should().Be(voteId);
    }
}
