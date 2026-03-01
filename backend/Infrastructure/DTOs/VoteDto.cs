namespace UrbaPF.Infrastructure.DTOs;

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