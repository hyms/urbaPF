using UrbaPF.Application.DTOs;
using UrbaPF.Domain.Entities;
using UrbaPF.Domain.Enums;

namespace UrbaPF.Application.Interfaces;

public interface IAuthApplicationService
{
    Task<(AuthResponseDto? Response, string? Error)> LoginAsync(LoginRequest request);
    Task<(Guid? UserId, string? Error)> RegisterAsync(RegisterRequest request);
    Task<(AuthResponseDto? Response, string? Error)> RefreshTokenAsync(RefreshTokenRequest request);
    Task<(bool Success, string? Error)> RevokeRefreshTokenAsync(string refreshToken);
    Task<(bool Success, string? Error)> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}

public interface IUserApplicationService
{
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<UserDto?> GetByEmailAsync(string email);
    Task<IEnumerable<UserDto>> GetByCondominiumAsync(Guid condominiumId);
    Task<(bool Success, string? Error)> UpdateAsync(Guid userId, UpdateUserDto dto, UserRole requesterRole);
    Task<(bool Success, string? Error)> UpdateRoleAsync(Guid userId, UserRole newRole, UserRole requesterRole);
    Task<(bool Success, string? Error)> UpdateCredibilityAsync(Guid userId, int newLevel, UserRole requesterRole);
    Task<(bool Success, string? Error)> DeleteAsync(Guid userId, UserRole requesterRole);
    Task<(bool Success, string? Error)> UploadPhotoAsync(Guid userId, Stream fileStream, string fileName, string contentType);
}

public interface ICondominiumApplicationService
{
    Task<CondominiumDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<CondominiumDto>> GetAllAsync();
    Task<(Guid? Id, string? Error)> CreateAsync(CreateCondominiumDto dto);
    Task<(bool Success, string? Error)> UpdateAsync(Guid id, UpdateCondominiumDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(Guid id);
}

public interface IPostApplicationService
{
    Task<PostDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<PostDto>> GetByCondominiumAsync(Guid condominiumId, int? status = null);
    Task<(Guid? Id, string? Error)> CreateAsync(Guid condominiumId, Guid userId, CreatePostDto dto);
    Task<(bool Success, string? Error)> UpdateAsync(Guid id, Guid userId, UserRole userRole, UpdatePostDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(Guid id, Guid userId, UserRole userRole);
    Task<(bool Success, string? Error)> ApproveAsync(Guid id, Guid approverId);
    Task<bool> IncrementViewCountAsync(Guid id);
}

public interface ICommentApplicationService
{
    Task<IEnumerable<CommentDto>> GetByPostAsync(Guid postId);
    Task<(Guid? Id, string? Error)> CreateAsync(Guid postId, Guid userId, CreateCommentDto dto);
    Task<(bool Success, string? Error)> UpdateAsync(Guid id, Guid userId, UserRole userRole, UpdateCommentDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(Guid id, Guid userId, UserRole userRole);
}

public interface IPollApplicationService
{
    Task<PollDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<PollDto>> GetByCondominiumAsync(Guid condominiumId, int? status = null);
    Task<(Guid? Id, string? Error)> CreateAsync(Guid condominiumId, Guid userId, UserRole userRole, CreatePollDto dto);
    Task<(bool Success, string? Error)> UpdateAsync(Guid id, Guid userId, UserRole userRole, UpdatePollDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(Guid id, Guid userId, UserRole userRole);
    Task<(bool Success, string? Error)> VoteAsync(Guid pollId, Guid userId, UserRole userRole, int optionIndex, string ipAddress);
    Task<VoteResultDto?> GetResultsAsync(Guid pollId);
    Task<(bool IsValid, string? Error)> VerifyVoteAsync(Guid pollId, Guid userId, int optionIndex, string signature);
}

public interface IAlertApplicationService
{
    Task<AlertDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<AlertDto>> GetByCondominiumAsync(Guid condominiumId, int? status = null);
    Task<IEnumerable<AlertDto>> GetActiveAlertsAsync(Guid condominiumId);
    Task<(Guid? Id, string? Error)> CreateAsync(Guid condominiumId, Guid creatorId, int creatorReputation, CreateAlertRequest dto);
    Task<(bool Success, string? Error)> ApproveAsync(Guid alertId, Guid approverId);
    Task<(bool Success, string? Error)> AcknowledgeAsync(Guid alertId, Guid userId);
    Task<(bool Success, string? Error)> ResolveAsync(Guid alertId, Guid userId);
    Task<(bool Success, string? Error)> DeleteAsync(Guid alertId, Guid userId, UserRole userRole);
    Task<(bool Success, string? Error)> ResendNotificationAsync(Guid alertId, Guid userId, UserRole userRole);
}

public interface IIncidentApplicationService
{
    Task<IncidentDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<IncidentDto>> GetByCondominiumAsync(Guid condominiumId, int? status = null);
    Task<(Guid? Id, string? Error)> CreateAsync(Guid condominiumId, Guid reporterId, CreateIncidentRequest dto);
    Task<(bool Success, string? Error)> UpdateAsync(Guid id, Guid userId, UserRole userRole, UpdateIncidentRequest dto);
    Task<(bool Success, string? Error)> UpdateStatusAsync(Guid id, UserRole userRole, bool isReporter, int newStatus, string? resolutionNotes);
    Task<(bool Success, string? Error)> DeleteAsync(Guid id, Guid userId, UserRole userRole);
}

public record AlertDto(
    Guid Id,
    Guid CondominiumId,
    Guid CreatorId,
    string CreatorName,
    int Type,
    string Title,
    string? Description,
    string? Location,
    int Status,
    int ReputationLevel,
    bool NeedsApproval,
    DateTime CreatedAt,
    DateTime? ApprovedAt,
    DateTime? AcknowledgedAt,
    DateTime? ResolvedAt
);

public record CreateIncidentRequest(
    string Title,
    string? Description,
    int Type,
    string? Location,
    string? AddressReference,
    List<IncidentMedia>? Media
);

public record UpdateIncidentRequest(
    string Title,
    string? Description,
    int Type,
    string? Location,
    string? AddressReference,
    List<IncidentMedia>? Media
);

public class IncidentDto
{
    public Guid Id { get; set; }
    public Guid CondominiumId { get; set; }
    public Guid ReporterId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Type { get; set; }
    public int Priority { get; set; }
    public int Status { get; set; }
    public string? Location { get; set; }
    public string? AddressReference { get; set; }
    public List<IncidentMedia>? Media { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}