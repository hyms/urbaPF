using System.Security.Cryptography;
using System.Text;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;

namespace UrbaPF.Infrastructure.Services;

public class PollService : IPollService
{
    private readonly IPollRepository _pollRepository;
    private readonly IVoteRepository _voteRepository;
    private readonly IAuditService _auditService;

    private const int RoleAdministrator = 4;
    private const int RoleManager = 3;
    private const int RoleNeighbor = 2;

    private const int PollStatusDraft = 1;
    private const int PollStatusScheduled = 2;
    private const int PollStatusActive = 3;
    private const int PollStatusClosed = 4;
    private const int PollStatusCancelled = 5;

    public PollService(IPollRepository pollRepository, IVoteRepository voteRepository, IAuditService auditService)
    {
        _pollRepository = pollRepository;
        _voteRepository = voteRepository;
        _auditService = auditService;
    }

    public async Task<PollDto?> GetByIdAsync(Guid id)
    {
        return await _pollRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<PollDto>> GetByCondominiumAsync(Guid condominiumId)
    {
        return await _pollRepository.GetByCondominiumAsync(condominiumId);
    }

    public async Task<(Guid pollId, int status)?> CreateAsync(CreatePollDto dto, Guid userId, Guid condominiumId, int userRole)
    {
        var initialStatus = (userRole == RoleManager || userRole == RoleAdministrator)
            ? PollStatusScheduled
            : PollStatusDraft;

        var pollId = await _pollRepository.CreateAsync(dto, userId, condominiumId, initialStatus);
        await _auditService.LogEventAsync(userId, condominiumId, "POLL_CREATED", pollId, dto);
        return (pollId, initialStatus);
    }

    public async Task<(bool success, string? error)> UpdateAsync(Guid id, UpdatePollDto dto, int userRole)
    {
        var poll = await _pollRepository.GetByIdAsync(id);
        if (poll is null)
            return (false, "Votación no encontrada");

        if (!CanEditPoll(poll.Status, userRole))
            return (false, "No se puede editar una votación activa o cerrada");

        await _pollRepository.UpdateAsync(id, dto);
        await _auditService.LogEventAsync(Guid.Empty, poll.CondominiumId, "POLL_UPDATED", id, dto);
        return (true, null);
    }

    public async Task<(bool success, string? error)> DeleteAsync(Guid id, int userRole)
    {
        if (userRole != RoleManager && userRole != RoleAdministrator)
            return (false, "No tienes permiso para eliminar votaciones");

        var poll = await _pollRepository.GetByIdAsync(id);
        if (poll is null)
            return (false, "Votación no encontrada");

        if (!CanEditPoll(poll.Status, userRole))
            return (false, "No se puede eliminar una votación activa o cerrada");

        var hasVoted = await _voteRepository.HasAnyVotesAsync(id);
        if (hasVoted)
            return (false, "No se puede eliminar una votación que ya tiene votos");

        await _pollRepository.SoftDeleteAsync(id);
        return (true, null);
    }

    public async Task<(bool success, string? error)> VoteAsync(Guid pollId, Guid userId, int userRole, int optionIndex, string ipAddress)
    {
        if (userRole != RoleNeighbor && userRole != RoleManager)
            return (false, "Solo vecinos y encargados pueden votar");

        var poll = await _pollRepository.GetByIdAsync(pollId);
        if (poll is null)
            return (false, "Votación no encontrada");

        if (poll.Status != PollStatusActive)
            return (false, "La votación no está activa");

        if (DateTime.UtcNow < poll.StartsAt || DateTime.UtcNow > poll.EndsAt)
            return (false, "La votación no está dentro del período establecido");

        if (userRole < poll.MinRoleToVote)
            return (false, "No tienes el rol mínimo requerido para votar en esta elección");

        var hasVoted = await _voteRepository.HasUserVotedAsync(pollId, userId);
        if (hasVoted)
            return (false, "Ya has votado en esta elección");

        var timestamp = DateTime.UtcNow;
        var signature = GenerateSignature(pollId, userId, optionIndex, timestamp);

        await _voteRepository.CreateAsync(userId, pollId, optionIndex, signature, ipAddress);
        return (true, null);
    }

    public async Task<VoteResultDto?> GetResultsAsync(Guid pollId)
    {
        var votes = await _voteRepository.GetByPollAsync(pollId);
        var results = await _voteRepository.GetResultsAsync(pollId);
        return new VoteResultDto
        {
            Votes = votes,
            Results = results
        };
    }

    public async Task<(bool isValid, string? error)> VerifyVoteAsync(Guid pollId, Guid userId, int optionIndex, string signature)
    {
        var poll = await _pollRepository.GetByIdAsync(pollId);
        if (poll is null)
            return (false, "Votación no encontrada");

        var votes = await _voteRepository.GetByPollAsync(pollId);
        var userVote = votes.FirstOrDefault(v => v.UserId == userId);

        if (userVote is null)
            return (false, "Usuario no ha votado");

        var expectedSignature = GenerateSignatureWithoutTimestamp(pollId, userId, optionIndex, poll.ServerSecret);
        var providedSignatureWithoutTimestamp = signature.Split(':')[0];

        if (userVote.DigitalSignature.StartsWith(expectedSignature))
            return (true, null);

        return (false, "Firma inválida");
    }

    private static bool CanEditPoll(int status, int userRole)
    {
        if (status == PollStatusActive || status == PollStatusClosed)
            return false;
        return userRole == RoleManager || userRole == RoleAdministrator;
    }

    private static string GenerateSignature(Guid pollId, Guid userId, int optionIndex, DateTime timestamp)
    {
        var data = $"{pollId}:{userId}:{optionIndex}:{timestamp:O}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }

    private static string GenerateSignatureWithoutTimestamp(Guid pollId, Guid userId, int optionIndex, string serverSecret)
    {
        var data = $"{pollId}:{userId}:{optionIndex}:{serverSecret}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }
}
