namespace UrbaPF.Infrastructure.DTOs;
using UrbaPF.Domain.Enums;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public UserRole Role { get; set; }
    public int CredibilityLevel { get; set; }
    public int Status { get; set; }
    public Guid? CondominiumId { get; set; }
    public string? LotNumber { get; set; }
    public string? StreetAddress { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsValidated { get; set; }
    public int ManagerVotes { get; set; }
    public string? FcmToken { get; set; }
    public bool ForcePasswordChange { get; set; }
}

public class CreateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public Guid? CondominiumId { get; set; }
    public string? LotNumber { get; set; }
    public string? StreetAddress { get; set; }
    public string? PhotoUrl { get; set; }
    public bool ForcePasswordChange { get; set; }
}

public class UpdateUserDto
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public UserRole? Role { get; set; }
    public int? CredibilityLevel { get; set; }
    public Guid? CondominiumId { get; set; }
    public string? LotNumber { get; set; }
    public string? StreetAddress { get; set; }
    public string? PhotoUrl { get; set; }
    public string? FcmToken { get; set; }
    public bool? ForcePasswordChange { get; set; }
}

public class ChangePasswordDto
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class UserSummaryDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? LotNumber { get; set; }
    public string? StreetAddress { get; set; }
    public string? PhotoUrl { get; set; }
    public int Role { get; set; }
    public bool IsValidated { get; set; }
    public int CredibilityLevel { get; set; }
}

public class UserDetailsDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? LotNumber { get; set; }
    public string? StreetAddress { get; set; }
    public string? PhotoUrl { get; set; }
    public int Role { get; set; }
    public int CredibilityLevel { get; set; }
    public int Status { get; set; }
    public bool IsValidated { get; set; }
    public int ManagerVotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool ForcePasswordChange { get; set; }
}
