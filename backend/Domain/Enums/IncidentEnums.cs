namespace UrbaPF.Domain.Enums;

public enum IncidentType
{
    Security = 1,
    Maintenance = 2,
    Noise = 3,
    Medical = 4,
    Fire = 5,
    SuspiciousActivity = 6,
    Vehicle = 7,
    CommonArea = 8,
    Other = 99
}

public enum IncidentPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4,
    Critical = 5
}

public enum IncidentStatus
{
    Reported = 1,
    Acknowledged = 2,
    InProgress = 3,
    Resolved = 4,
    Closed = 5,
    Cancelled = 6
}
