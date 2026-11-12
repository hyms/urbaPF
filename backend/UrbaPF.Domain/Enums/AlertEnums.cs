namespace UrbaPF.Domain.Enums;

public enum AlertStatus
{
    Pending = 1,
    Approved = 2,
    Notified = 3,
    InProgress = 4,
    Resolved = 5,
    Cancelled = 6
}

public enum AlertType
{
    Emergency = 1,
    Robbery = 2,
    Fire = 3,
    Medical = 4,
    Other = 5
}