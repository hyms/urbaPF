namespace UrbaPF.Infrastructure.DTOs;

public class AlertDto
{
    public Guid Id { get; set; }
    public Guid CondominiumId { get; set; }
    public Guid CreatedById { get; set; }
    public string? CreatedByName { get; set; }
    public int AlertType { get; set; }
    public string Message { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? DestinationAddress { get; set; }
    public DateTime EstimatedArrival { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? ArrivedAt { get; set; }
}

public class CreateAlertDto
{
    public int AlertType { get; set; }
    public string Message { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? DestinationAddress { get; set; }
    public DateTime EstimatedArrival { get; set; }
}

public class UpdateAlertStatusDto
{
    public int Status { get; set; }
}

public class UpdateAlertDto
{
    public string? Message { get; set; }
    public int? AlertType { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? DestinationAddress { get; set; }
    public DateTime? EstimatedArrival { get; set; }
}
