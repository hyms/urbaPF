namespace UrbaPF.Domain.Entities;

public class Incident
{
    public Guid Id { get; set; }
    public Guid CondominiumId { get; set; }
    public Guid ReporterId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Type { get; set; } = 1;
    public int Priority { get; set; } = 2;
    public int Status { get; set; } = 1;
    public string? Location { get; set; }
    public string? AddressReference { get; set; }
    public string? Media { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public class IncidentMedia
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = "image";
    public string Path { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class IncidentComment
{
    public Guid Id { get; set; }
    public Guid IncidentId { get; set; }
    public Guid AuthorId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
