namespace UrbaPF.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int Role { get; set; }
    public int CredibilityLevel { get; set; } = 1;
    public int Status { get; set; }
    public Guid? CondominiumId { get; set; }
    public string? LotNumber { get; set; }
    public string? StreetAddress { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? FcmToken { get; set; }
    public bool IsValidated { get; set; }
    public int ManagerVotes { get; set; }
}
