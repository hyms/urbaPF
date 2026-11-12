namespace UrbaPF.Domain.Enums;

public enum PollType
{
    SingleChoice = 1,
    MultipleChoice = 2,
    YesNo = 3,
    Rating = 4
}

public enum PollStatus
{
    Draft = 1,
    Scheduled = 2,
    Active = 3,
    Closed = 4,
    Cancelled = 5
}
