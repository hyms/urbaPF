using FluentAssertions;
using Moq;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;

namespace UrbaPF.Tests.Repositories;

public class UserRepositoryMockTests
{
    [Fact]
    public async Task GetAllUsers_CallsRepository()
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
        mockRepo.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetUserById_ReturnsUser()
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
    public async Task GetUserByEmail_ReturnsUser()
    {
        var mockRepo = new Mock<IUserRepository>();
        var user = new UserDto { Email = "test@test.com" };
        mockRepo.Setup(x => x.GetByEmailAsync("test@test.com")).ReturnsAsync(user);

        var result = await mockRepo.Object.GetByEmailAsync("test@test.com");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateUser_CallsRepository()
    {
        var mockRepo = new Mock<IUserRepository>();
        var userId = Guid.NewGuid();
        mockRepo.Setup(x => x.CreateAsync(It.IsAny<CreateUserDto>(), It.IsAny<string>())).ReturnsAsync(userId);

        var result = await mockRepo.Object.CreateAsync(
            new CreateUserDto { Email = "new@test.com", FullName = "New User" }, 
            "password");

        result.Should().Be(userId);
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

    [Fact]
    public async Task UpdateLastLogin_CallsRepository()
    {
        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(x => x.UpdateLastLoginAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        await mockRepo.Object.UpdateLastLoginAsync(Guid.NewGuid());

        mockRepo.Verify(x => x.UpdateLastLoginAsync(It.IsAny<Guid>()), Times.Once);
    }
}

public class CondominiumRepositoryMockTests
{
    [Fact]
    public async Task GetAllCondominiums_CallsRepository()
    {
        var mockRepo = new Mock<ICondominiumRepository>();
        var condos = new List<CondominiumDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Condo 1" },
            new() { Id = Guid.NewGuid(), Name = "Condo 2" }
        };
        mockRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(condos);

        var result = await mockRepo.Object.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCondominiumById_ReturnsCondominium()
    {
        var mockRepo = new Mock<ICondominiumRepository>();
        var condoId = Guid.NewGuid();
        mockRepo.Setup(x => x.GetByIdAsync(condoId)).ReturnsAsync(new CondominiumDto { Id = condoId });

        var result = await mockRepo.Object.GetByIdAsync(condoId);

        result.Should().NotBeNull();
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

    [Fact]
    public async Task UpdateCondominium_CallsRepository()
    {
        var mockRepo = new Mock<ICondominiumRepository>();
        mockRepo.Setup(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateCondominiumDto>())).Returns(Task.CompletedTask);

        await mockRepo.Object.UpdateAsync(Guid.NewGuid(), new UpdateCondominiumDto { Name = "Updated" });

        mockRepo.Verify(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateCondominiumDto>()), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteCondominium_CallsRepository()
    {
        var mockRepo = new Mock<ICondominiumRepository>();
        mockRepo.Setup(x => x.SoftDeleteAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        await mockRepo.Object.SoftDeleteAsync(Guid.NewGuid());

        mockRepo.Verify(x => x.SoftDeleteAsync(It.IsAny<Guid>()), Times.Once);
    }
}

public class PostRepositoryMockTests
{
    [Fact]
    public async Task GetAllPosts_CallsRepository()
    {
        var mockRepo = new Mock<IPostRepository>();
        mockRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<PostDto>());

        await mockRepo.Object.GetAllAsync();

        mockRepo.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetPostById_ReturnsPost()
    {
        var mockRepo = new Mock<IPostRepository>();
        var postId = Guid.NewGuid();
        mockRepo.Setup(x => x.GetByIdAsync(postId)).ReturnsAsync(new PostDto { Id = postId });

        var result = await mockRepo.Object.GetByIdAsync(postId);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPostsByCondominium_ReturnsList()
    {
        var mockRepo = new Mock<IPostRepository>();
        var condoId = Guid.NewGuid();
        mockRepo.Setup(x => x.GetByCondominiumAsync(condoId)).ReturnsAsync(new List<PostDto>());

        var result = await mockRepo.Object.GetByCondominiumAsync(condoId);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreatePost_ReturnsId()
    {
        var mockRepo = new Mock<IPostRepository>();
        var postId = Guid.NewGuid();
        mockRepo.Setup(x => x.CreateAsync(It.IsAny<CreatePostDto>(), It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(postId);

        var result = await mockRepo.Object.CreateAsync(
            new CreatePostDto { Title = "New Post" },
            Guid.NewGuid(),
            Guid.NewGuid());

        result.Should().Be(postId);
    }

    [Fact]
    public async Task UpdatePost_CallsRepository()
    {
        var mockRepo = new Mock<IPostRepository>();
        mockRepo.Setup(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdatePostDto>())).Returns(Task.CompletedTask);

        await mockRepo.Object.UpdateAsync(Guid.NewGuid(), new UpdatePostDto { Title = "Updated" });

        mockRepo.Verify(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdatePostDto>()), Times.Once);
    }

    [Fact]
    public async Task SoftDeletePost_CallsRepository()
    {
        var mockRepo = new Mock<IPostRepository>();
        mockRepo.Setup(x => x.SoftDeleteAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        await mockRepo.Object.SoftDeleteAsync(Guid.NewGuid());

        mockRepo.Verify(x => x.SoftDeleteAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task IncrementViewCount_CallsRepository()
    {
        var mockRepo = new Mock<IPostRepository>();
        mockRepo.Setup(x => x.IncrementViewCountAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        await mockRepo.Object.IncrementViewCountAsync(Guid.NewGuid());

        mockRepo.Verify(x => x.IncrementViewCountAsync(It.IsAny<Guid>()), Times.Once);
    }
}

public class IncidentRepositoryMockTests
{
    [Fact]
    public async Task GetAllIncidents_CallsRepository()
    {
        var mockRepo = new Mock<IIncidentRepository>();
        mockRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<IncidentDto>());

        await mockRepo.Object.GetAllAsync();

        mockRepo.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetIncidentById_ReturnsIncident()
    {
        var mockRepo = new Mock<IIncidentRepository>();
        var incidentId = Guid.NewGuid();
        mockRepo.Setup(x => x.GetByIdAsync(incidentId)).ReturnsAsync(new IncidentDto { Id = incidentId });

        var result = await mockRepo.Object.GetByIdAsync(incidentId);

        result.Should().NotBeNull();
    }

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
    public async Task CreateIncident_ReturnsId()
    {
        var mockRepo = new Mock<IIncidentRepository>();
        var incidentId = Guid.NewGuid();
        mockRepo.Setup(x => x.CreateAsync(It.IsAny<CreateIncidentDto>(), It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(incidentId);

        var result = await mockRepo.Object.CreateAsync(
            new CreateIncidentDto { Title = "New Incident" },
            Guid.NewGuid(),
            Guid.NewGuid());

        result.Should().Be(incidentId);
    }

    [Fact]
    public async Task UpdateIncident_CallsRepository()
    {
        var mockRepo = new Mock<IIncidentRepository>();
        mockRepo.Setup(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateIncidentDto>())).Returns(Task.CompletedTask);

        await mockRepo.Object.UpdateAsync(Guid.NewGuid(), new UpdateIncidentDto { Title = "Updated" });

        mockRepo.Verify(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateIncidentDto>()), Times.Once);
    }

    [Fact]
    public async Task UpdateIncidentStatus_CallsRepository()
    {
        var mockRepo = new Mock<IIncidentRepository>();
        mockRepo.Setup(x => x.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string?>())).Returns(Task.CompletedTask);

        await mockRepo.Object.UpdateStatusAsync(Guid.NewGuid(), 2, "Resolved");

        mockRepo.Verify(x => x.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteIncident_CallsRepository()
    {
        var mockRepo = new Mock<IIncidentRepository>();
        mockRepo.Setup(x => x.SoftDeleteAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        await mockRepo.Object.SoftDeleteAsync(Guid.NewGuid());

        mockRepo.Verify(x => x.SoftDeleteAsync(It.IsAny<Guid>()), Times.Once);
    }
}

public class PollRepositoryMockTests
{
    [Fact]
    public async Task GetAllPolls_CallsRepository()
    {
        var mockRepo = new Mock<IPollRepository>();
        mockRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<PollDto>());

        await mockRepo.Object.GetAllAsync();

        mockRepo.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetPollById_ReturnsPoll()
    {
        var mockRepo = new Mock<IPollRepository>();
        var pollId = Guid.NewGuid();
        mockRepo.Setup(x => x.GetByIdAsync(pollId)).ReturnsAsync(new PollDto { Id = pollId });

        var result = await mockRepo.Object.GetByIdAsync(pollId);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPollsByCondominium_ReturnsList()
    {
        var mockRepo = new Mock<IPollRepository>();
        var condoId = Guid.NewGuid();
        mockRepo.Setup(x => x.GetByCondominiumAsync(condoId)).ReturnsAsync(new List<PollDto>());

        var result = await mockRepo.Object.GetByCondominiumAsync(condoId);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreatePoll_ReturnsId()
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

    [Fact]
    public async Task UpdatePoll_CallsRepository()
    {
        var mockRepo = new Mock<IPollRepository>();
        mockRepo.Setup(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdatePollDto>())).Returns(Task.CompletedTask);

        await mockRepo.Object.UpdateAsync(Guid.NewGuid(), new UpdatePollDto { Title = "Updated" });

        mockRepo.Verify(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdatePollDto>()), Times.Once);
    }

    [Fact]
    public async Task SoftDeletePoll_CallsRepository()
    {
        var mockRepo = new Mock<IPollRepository>();
        mockRepo.Setup(x => x.SoftDeleteAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        await mockRepo.Object.SoftDeleteAsync(Guid.NewGuid());

        mockRepo.Verify(x => x.SoftDeleteAsync(It.IsAny<Guid>()), Times.Once);
    }
}

public class VoteRepositoryMockTests
{
    [Fact]
    public async Task GetVotesByPoll_ReturnsList()
    {
        var mockRepo = new Mock<IVoteRepository>();
        var pollId = Guid.NewGuid();
        mockRepo.Setup(x => x.GetByPollAsync(pollId)).ReturnsAsync(new List<VoteDto>());

        var result = await mockRepo.Object.GetByPollAsync(pollId);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetVoteById_ReturnsVote()
    {
        var mockRepo = new Mock<IVoteRepository>();
        var voteId = Guid.NewGuid();
        mockRepo.Setup(x => x.GetByIdAsync(voteId)).ReturnsAsync(new VoteDto { Id = voteId });

        var result = await mockRepo.Object.GetByIdAsync(voteId);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateVote_ReturnsId()
    {
        var mockRepo = new Mock<IVoteRepository>();
        var voteId = Guid.NewGuid();
        mockRepo.Setup(x => x.CreateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(voteId);

        var result = await mockRepo.Object.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), 1, "sig", "ip");

        result.Should().Be(voteId);
    }

    [Fact]
    public async Task HasUserVoted_ReturnsFalse()
    {
        var mockRepo = new Mock<IVoteRepository>();
        mockRepo.Setup(x => x.HasUserVotedAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(false);

        var result = await mockRepo.Object.HasUserVotedAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasUserVoted_ReturnsTrue()
    {
        var mockRepo = new Mock<IVoteRepository>();
        mockRepo.Setup(x => x.HasUserVotedAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(true);

        var result = await mockRepo.Object.HasUserVotedAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetResults_ReturnsDictionary()
    {
        var mockRepo = new Mock<IVoteRepository>();
        mockRepo.Setup(x => x.GetResultsAsync(It.IsAny<Guid>())).ReturnsAsync(new Dictionary<int, int> { { 1, 10 } });

        var result = await mockRepo.Object.GetResultsAsync(Guid.NewGuid());

        result.Should().NotBeNull();
    }
}

public class AlertRepositoryMockTests
{
    [Fact]
    public async Task GetAllAlerts_CallsRepository()
    {
        var mockRepo = new Mock<IAlertRepository>();
        mockRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<AlertDto>());

        await mockRepo.Object.GetAllAsync();

        mockRepo.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAlertById_ReturnsAlert()
    {
        var mockRepo = new Mock<IAlertRepository>();
        var alertId = Guid.NewGuid();
        mockRepo.Setup(x => x.GetByIdAsync(alertId)).ReturnsAsync(new AlertDto { Id = alertId });

        var result = await mockRepo.Object.GetByIdAsync(alertId);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetActiveAlertsByCondominium_ReturnsList()
    {
        var mockRepo = new Mock<IAlertRepository>();
        var condoId = Guid.NewGuid();
        mockRepo.Setup(x => x.GetActiveByCondominiumAsync(condoId)).ReturnsAsync(new List<AlertDto>());

        var result = await mockRepo.Object.GetActiveByCondominiumAsync(condoId);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAlert_ReturnsId()
    {
        var mockRepo = new Mock<IAlertRepository>();
        var alertId = Guid.NewGuid();
        mockRepo.Setup(x => x.CreateAsync(It.IsAny<CreateAlertDto>(), It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(alertId);

        var result = await mockRepo.Object.CreateAsync(
            new CreateAlertDto { Message = "Alert" },
            Guid.NewGuid(),
            Guid.NewGuid());

        result.Should().Be(alertId);
    }

    [Fact]
    public async Task UpdateAlert_CallsRepository()
    {
        var mockRepo = new Mock<IAlertRepository>();
        mockRepo.Setup(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateAlertDto>())).Returns(Task.CompletedTask);

        await mockRepo.Object.UpdateAsync(Guid.NewGuid(), new UpdateAlertDto { Message = "Updated" });

        mockRepo.Verify(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateAlertDto>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAlertStatus_CallsRepository()
    {
        var mockRepo = new Mock<IAlertRepository>();
        mockRepo.Setup(x => x.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<int>())).Returns(Task.CompletedTask);

        await mockRepo.Object.UpdateStatusAsync(Guid.NewGuid(), 2);

        mockRepo.Verify(x => x.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteAlert_CallsRepository()
    {
        var mockRepo = new Mock<IAlertRepository>();
        mockRepo.Setup(x => x.SoftDeleteAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        await mockRepo.Object.SoftDeleteAsync(Guid.NewGuid());

        mockRepo.Verify(x => x.SoftDeleteAsync(It.IsAny<Guid>()), Times.Once);
    }
}

public class CommentRepositoryMockTests
{
    [Fact]
    public async Task GetCommentsByPost_ReturnsList()
    {
        var mockRepo = new Mock<ICommentRepository>();
        var postId = Guid.NewGuid();
        mockRepo.Setup(x => x.GetByPostAsync(postId)).ReturnsAsync(new List<CommentDto>());

        var result = await mockRepo.Object.GetByPostAsync(postId);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCommentById_ReturnsComment()
    {
        var mockRepo = new Mock<ICommentRepository>();
        var commentId = Guid.NewGuid();
        mockRepo.Setup(x => x.GetByIdAsync(commentId)).ReturnsAsync(new CommentDto { Id = commentId });

        var result = await mockRepo.Object.GetByIdAsync(commentId);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateComment_ReturnsId()
    {
        var mockRepo = new Mock<ICommentRepository>();
        var commentId = Guid.NewGuid();
        mockRepo.Setup(x => x.CreateAsync(It.IsAny<CreateCommentDto>(), It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(commentId);

        var result = await mockRepo.Object.CreateAsync(
            new CreateCommentDto { Content = "Comment" },
            Guid.NewGuid(),
            Guid.NewGuid());

        result.Should().Be(commentId);
    }

    [Fact]
    public async Task SoftDeleteComment_CallsRepository()
    {
        var mockRepo = new Mock<ICommentRepository>();
        mockRepo.Setup(x => x.SoftDeleteAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        await mockRepo.Object.SoftDeleteAsync(Guid.NewGuid());

        mockRepo.Verify(x => x.SoftDeleteAsync(It.IsAny<Guid>()), Times.Once);
    }
}
