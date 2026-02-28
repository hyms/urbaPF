namespace UrbaPF.Domain.Enums;

public enum AlertType
{
    OnMyWay = 1,
    Emergency = 2,
    CheckIn = 3,
    Patrol = 4
}

public enum AlertStatus
{
    Pending = 1,
    Acknowledged = 2,
    InTransit = 3,
    Arrived = 4,
    Cancelled = 5,
    Completed = 6
}
