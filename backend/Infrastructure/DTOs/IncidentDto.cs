namespace UrbaPF.Infrastructure.DTOs;

public class IncidentDto
{
    public Guid Id { get; set; }
    public Guid CondominiumId { get; set; }
    public Guid ReportedById { get; set; }
    public string? ReportedByName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Type { get; set; }
    public int Priority { get; set; }
    public int Status { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? LocationDescription { get; set; }
    public DateTime OccurredAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
}

public class CreateIncidentDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Type { get; set; }
    public int Priority { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? LocationDescription { get; set; }
    public DateTime OccurredAt { get; set; }
}

public class UpdateIncidentStatusDto
{
    public int Status { get; set; }
    public string? ResolutionNotes { get; set; }
}

public class UpdateIncidentDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? Type { get; set; }
    public int? Priority { get; set; }
}
