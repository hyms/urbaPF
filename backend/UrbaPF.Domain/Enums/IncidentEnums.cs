namespace UrbaPF.Domain.Enums;

public enum IncidentType
{
    Maintenance = 1,
    Security = 2,
    Cleaning = 3,
    Infrastructure = 4,
    Other = 5
}

public enum IncidentPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4
}

public enum IncidentStatus
{
    Reported = 1,
    InProgress = 2,
    Pending = 3,
    Resolved = 4,
    Closed = 5,
    Cancelled = 6
}