using UrbaPF.Domain.Enums;

namespace UrbaPF.Api.DTOs;

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Email, string Password, string FullName, string? Phone);
public record RefreshTokenRequest(string RefreshToken);
public record CreateUserRequest(string Email, string Password, string FullName, string? Phone);
public record UpdateUserRequest(string? FullName, string? Phone, string? FcmToken, string? StreetAddress, string? PhotoUrl, UserRole? Role = null);
public record ChangePasswordRequest(string OldPassword, string NewPassword);
public record CreateCondominiumRequest(string Name, string Address, string? Description, string? Rules, decimal MonthlyFee, string? Currency);
public record UpdateCondominiumRequest(string? Name, string? Address, string? Description, string? Rules, decimal? MonthlyFee, bool? IsActive);
public record CreatePostRequest(string Title, string Content, bool IsPinned, bool IsAnnouncement, int Category);
public record UpdatePostRequest(string? Title, string? Content, bool? IsPinned, bool? IsAnnouncement, int? Status, int? Category);
public record CreateCommentRequest(Guid? ParentCommentId, string Content);
public record CreateIncidentRequest(string Title, string Description, int Type, int Priority, double? Latitude, double? Longitude, string? LocationDescription, DateTime OccurredAt);
public record UpdateIncidentStatusRequest(int Status, string? ResolutionNotes);
public record CreatePollRequest(string Title, string? Description, string Options, int PollType, DateTime StartsAt, DateTime EndsAt, bool RequiresJustification, int MinRoleToVote);
public record UpdatePollRequest(string? Title, string? Description, string? Options, DateTime? StartsAt, DateTime? EndsAt, int? Status);
public record CreateVoteRequest(int OptionIndex);
public record CreateAlertRequest(int AlertType, string Message, double? Latitude, double? Longitude, string? DestinationAddress, DateTime EstimatedArrival);
public record UpdateAlertStatusRequest(int Status);
public record CreateExpenseRequest(Guid CondominiumId, string Description, decimal Amount, string Category, DateTime DueDate);
public record UpdateExpenseRequest(string? Description, decimal? Amount, string? Category, DateTime? DueDate, int? Status, string? ReceiptUrl);
public record PayExpenseRequest(Guid PaidById, string? ReceiptUrl);