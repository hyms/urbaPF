using UrbaPF.Domain.Enums;

namespace UrbaPF.Application.DTOs;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = new();
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public UserRole Role { get; set; }
    public int CredibilityLevel { get; set; }
    public int Status { get; set; }
    public Guid? CondominiumId { get; set; }
    public string? LotNumber { get; set; }
    public string? StreetAddress { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsValidated { get; set; }
    public int ManagerVotes { get; set; }
    public string? FcmToken { get; set; }
}

public class CreateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public Guid? CondominiumId { get; set; }
    public string? LotNumber { get; set; }
    public string? StreetAddress { get; set; }
    public string? PhotoUrl { get; set; }
}

public class UpdateUserDto
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public UserRole? Role { get; set; }
    public int? CredibilityLevel { get; set; }
    public Guid? CondominiumId { get; set; }
    public string? LotNumber { get; set; }
    public string? StreetAddress { get; set; }
    public string? PhotoUrl { get; set; }
    public string? FcmToken { get; set; }
}

public class CondominiumDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public string? Rules { get; set; }
    public decimal MonthlyFee { get; set; }
    public string Currency { get; set; } = "BOB";
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class CreateCondominiumDto
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Rules { get; set; }
    public decimal MonthlyFee { get; set; }
    public string? Currency { get; set; }
}

public class UpdateCondominiumDto
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public string? Rules { get; set; }
    public decimal? MonthlyFee { get; set; }
    public bool? IsActive { get; set; }
}

public class PostDto
{
    public Guid Id { get; set; }
    public Guid CondominiumId { get; set; }
    public Guid AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Category { get; set; }
    public bool IsPinned { get; set; }
    public bool IsAnnouncement { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ViewCount { get; set; }
}

public class CreatePostDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Category { get; set; }
    public bool IsPinned { get; set; }
    public bool IsAnnouncement { get; set; }
    public int Status { get; set; } = 0;
}

public class UpdatePostDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public int? Category { get; set; }
    public bool? IsPinned { get; set; }
    public bool? IsAnnouncement { get; set; }
    public int? Status { get; set; }
}

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int CredibilityLevel { get; set; }
    public bool IsHidden { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCommentDto
{
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class UpdateCommentDto
{
    public string? Content { get; set; }
    public bool? IsHidden { get; set; }
}

public class PollDto
{
    public Guid Id { get; set; }
    public Guid CondominiumId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Options { get; set; } = "[]";
    public int PollType { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public bool RequiresJustification { get; set; }
    public int MinRoleToVote { get; set; }
    public string ServerSecret { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedById { get; set; }
    public string? CreatedByName { get; set; }
}

public class CreatePollDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Options { get; set; } = "[]";
    public int PollType { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public bool RequiresJustification { get; set; }
    public int MinRoleToVote { get; set; }
    public int Status { get; set; }
}

public class UpdatePollDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Options { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public int? Status { get; set; }
}

public class VoteDto
{
    public Guid Id { get; set; }
    public Guid PollId { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public int OptionIndex { get; set; }
    public string DigitalSignature { get; set; } = string.Empty;
    public DateTime VotedAt { get; set; }
}

public class CreateVoteDto
{
    public int OptionIndex { get; set; }
}

public class VoteResultDto
{
    public IEnumerable<VoteDto> Votes { get; set; } = [];
    public Dictionary<int, int> Results { get; set; } = new();
}