namespace UrbaPF.Domain.Enums;

public enum PostCategory
{
    General = 1,
    Announcement = 2,
    Maintenance = 3,
    Security = 4,
    Social = 5,
    Complaint = 6,
    Suggestion = 7,
    LostAndFound = 8,
    ForSale = 9,
    Events = 10
}

public enum PostStatus
{
    Draft = 1,
    PendingApproval = 2,
    Published = 3,
    Rejected = 4,
    Archived = 5
}
