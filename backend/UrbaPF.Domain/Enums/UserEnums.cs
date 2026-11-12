namespace UrbaPF.Domain.Enums;

public enum UserRole
{
    Restricted = 0,
    Guard = 1,
    Neighbor = 2,
    Manager = 3,
    Administrator = 4
}

public enum CredibilityLevel
{
    Irreverent = 1,
    Newbie = 2,
    Regular = 3,
    Trusted = 4,
    Elder = 5
}

public enum UserStatus
{
    Pending = 0,
    Active = 1,
    Suspended = 2,
    Disabled = 3
}
