namespace UrbaPF.Domain.Entities;

public class Alert
{
    public Guid Id { get; set; }
    public Guid CondominiumId { get; set; }
    public Guid CreatorId { get; set; }
    public int Type { get; set; } = 1;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public int Status { get; set; } = 1;
    public int ReputationLevel { get; set; } = 1;
    public bool NeedsApproval { get; set; }
    public Guid? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? NotifiedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? AcknowledgedById { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public class AlertNotification
{
    public Guid Id { get; set; }
    public Guid AlertId { get; set; }
    public Guid UserId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
