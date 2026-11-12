namespace UrbaPF.Domain.Entities;

public class Vote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PollId { get; set; }
    public Guid UserId { get; set; }
    public int OptionIndex { get; set; }
    public string DigitalSignature { get; set; } = string.Empty;
    public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; } = string.Empty;
}
