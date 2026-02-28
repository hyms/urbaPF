namespace UrbaPF.Domain.Entities;

public class Poll
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CondominiumId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Options { get; set; } = "[]";
    public int PollType { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public bool RequiresJustification { get; set; }
    public int MinRoleToVote { get; set; }
    public string ServerSecret { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedById { get; set; }
}
