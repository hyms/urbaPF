using FluentAssertions;
using UrbaPF.Infrastructure.DTOs;

namespace UrbaPF.Tests.DTOs;

public class UserDtoTests
{
    [Fact]
    public void UserDto_DefaultValues_AreCorrect()
    {
        var dto = new UserDto();

        dto.Email.Should().BeEmpty();
        dto.FullName.Should().BeEmpty();
        dto.Role.Should().Be(0);
        dto.CredibilityLevel.Should().Be(0);
        dto.Status.Should().Be(0);
    }

    [Fact]
    public void CreateUserDto_CanSetProperties()
    {
        var dto = new CreateUserDto
        {
            Email = "test@test.com",
            FullName = "Test User",
            Phone = "12345678",
            CondominiumId = Guid.NewGuid(),
            LotNumber = "A1",
            StreetAddress = "Calle 1"
        };

        dto.Email.Should().Be("test@test.com");
        dto.FullName.Should().Be("Test User");
        dto.Phone.Should().Be("12345678");
    }

    [Fact]
    public void UpdateUserDto_PropertiesAreNullable()
    {
        var dto = new UpdateUserDto();

        dto.FullName.Should().BeNull();
        dto.Phone.Should().BeNull();
        dto.FcmToken.Should().BeNull();
    }
}

public class CondominiumDtoTests
{
    [Fact]
    public void CondominiumDto_DefaultCurrency_IsBOB()
    {
        var dto = new CondominiumDto();

        dto.Currency.Should().Be("BOB");
    }

    [Fact]
    public void CreateCondominiumDto_CanSetProperties()
    {
        var dto = new CreateCondominiumDto
        {
            Name = "Condominio Test",
            Address = "Dirección Test",
            Description = "Descripción",
            MonthlyFee = 500.00m,
            Currency = "BOB"
        };

        dto.Name.Should().Be("Condominio Test");
        dto.MonthlyFee.Should().Be(500.00m);
    }

    [Fact]
    public void UpdateCondominiumDto_MonthlyFee_IsNullable()
    {
        var dto = new UpdateCondominiumDto { MonthlyFee = 600.00m };

        dto.MonthlyFee.Should().Be(600.00m);
    }
}

public class PostDtoTests
{
    [Fact]
    public void PostDto_DefaultValues_AreCorrect()
    {
        var dto = new PostDto();

        dto.Title.Should().BeEmpty();
        dto.Content.Should().BeEmpty();
        dto.IsPinned.Should().BeFalse();
        dto.IsAnnouncement.Should().BeFalse();
        dto.Status.Should().Be(0);
    }

    [Fact]
    public void CreatePostDto_CanSetProperties()
    {
        var dto = new CreatePostDto
        {
            Title = "Test Post",
            Content = "Test Content",
            Category = 1,
            IsPinned = true,
            IsAnnouncement = false
        };

        dto.Title.Should().Be("Test Post");
        dto.IsPinned.Should().BeTrue();
    }

    [Fact]
    public void UpdatePostDto_PropertiesAreNullable()
    {
        var dto = new UpdatePostDto { Title = "Updated", IsPinned = true };

        dto.Title.Should().Be("Updated");
        dto.Content.Should().BeNull();
    }
}

public class CommentDtoTests
{
    [Fact]
    public void CommentDto_DefaultCredibilityLevel_IsZero()
    {
        var dto = new CommentDto();

        dto.CredibilityLevel.Should().Be(0);
        dto.IsHidden.Should().BeFalse();
    }

    [Fact]
    public void CreateCommentDto_CanSetProperties()
    {
        var dto = new CreateCommentDto
        {
            Content = "Test Comment",
            ParentCommentId = Guid.NewGuid()
        };

        dto.Content.Should().Be("Test Comment");
        dto.ParentCommentId.Should().NotBeNull();
    }
}

public class IncidentDtoTests
{
    [Fact]
    public void IncidentDto_CanSetProperties()
    {
        var dto = new IncidentDto
        {
            Title = "Incident Test",
            Description = "Description",
            Type = 1,
            Priority = 2,
            Status = 1,
            Latitude = -17.8146,
            Longitude = -63.1561
        };

        dto.Title.Should().Be("Incident Test");
        dto.Latitude.Should().Be(-17.8146);
    }

    [Fact]
    public void CreateIncidentDto_CanSetProperties()
    {
        var dto = new CreateIncidentDto
        {
            Title = "New Incident",
            OccurredAt = DateTime.UtcNow
        };

        dto.Title.Should().Be("New Incident");
        dto.OccurredAt.Should().NotBe(default);
    }
}

public class PollDtoTests
{
    [Fact]
    public void PollDto_DefaultOptions_IsEmptyArray()
    {
        var dto = new PollDto();

        dto.Options.Should().Be("[]");
    }

    [Fact]
    public void CreatePollDto_CanSetProperties()
    {
        var dto = new CreatePollDto
        {
            Title = "Test Poll",
            Options = "[\"Option1\",\"Option2\"]",
            PollType = 1,
            StartsAt = DateTime.UtcNow,
            EndsAt = DateTime.UtcNow.AddDays(7),
            RequiresJustification = false,
            MinRoleToVote = 1
        };

        dto.Title.Should().Be("Test Poll");
        dto.PollType.Should().Be(1);
    }

    [Fact]
    public void VoteDto_CanSetProperties()
    {
        var dto = new VoteDto
        {
            OptionIndex = 1,
            DigitalSignature = "signature123"
        };

        dto.OptionIndex.Should().Be(1);
        dto.DigitalSignature.Should().Be("signature123");
    }
}

public class AlertDtoTests
{
    [Fact]
    public void AlertDto_CanSetProperties()
    {
        var dto = new AlertDto
        {
            AlertType = 1,
            Message = "Security Alert",
            Status = 1,
            EstimatedArrival = DateTime.UtcNow.AddMinutes(30)
        };

        dto.AlertType.Should().Be(1);
        dto.Message.Should().Be("Security Alert");
    }

    [Fact]
    public void CreateAlertDto_CanSetProperties()
    {
        var dto = new CreateAlertDto
        {
            AlertType = 2,
            Message = "Emergency",
            EstimatedArrival = DateTime.UtcNow
        };

        dto.AlertType.Should().Be(2);
    }
}
