namespace UrbaPF.Infrastructure.DTOs;

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
