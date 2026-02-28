namespace UrbaPF.Domain.Entities;

public class Alert
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CondominiumId { get; set; }
    public Guid CreatedById { get; set; }
    public int AlertType { get; set; }
    public string Message { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? DestinationAddress { get; set; }
    public DateTime EstimatedArrival { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? ArrivedAt { get; set; }
}
