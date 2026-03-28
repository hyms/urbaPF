using Dapper;
using UrbaPF.Infrastructure.Data;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;

namespace UrbaPF.Infrastructure.Repositories;

public class VoteRepository : BaseRepository, IVoteRepository
{
    public VoteRepository(DbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<IEnumerable<VoteDto>> GetByPollAsync(Guid pollId)
    {
        var sql = @"
            SELECT v.id, v.poll_id as PollId, v.user_id as UserId, v.option_index as OptionIndex,
                   v.digital_signature as DigitalSignature, v.justification, v.voted_at as VotedAt,
                   v.ip_address as IpAddress,
                   u.full_name as UserName
            FROM votes v
            JOIN users u ON v.user_id = u.id
            WHERE v.poll_id = @PollId AND deleted_at IS NULL
            ORDER BY v.voted_at DESC";
        return await QueryAsync<VoteDto>(sql, new { PollId = pollId });
    }

    public async Task<VoteDto?> GetByIdAsync(Guid id)
    {
        var sql = @"
            SELECT v.id, v.poll_id as PollId, v.user_id as UserId, v.option_index as OptionIndex,
                   v.digital_signature as DigitalSignature, v.justification, v.voted_at as VotedAt,
                   v.ip_address as IpAddress,
                   u.full_name as UserName
            FROM votes v
            JOIN users u ON v.user_id = u.id
            WHERE v.id = @Id AND v.deleted_at IS NULL";
        return await QueryFirstOrDefaultAsync<VoteDto>(sql, new { Id = id });
    }

    public async Task<Guid> CreateAsync(Guid userId, Guid pollId, int optionIndex, string signature, string ipAddress)
    {
        var sql = @"
            INSERT INTO votes (id, poll_id, user_id, option_index, digital_signature, ip_address)
            VALUES (gen_random_uuid(), @PollId, @UserId, @OptionIndex, @DigitalSignature, @IpAddress)
            RETURNING id";
        return await ExecuteScalarAsync<Guid>(sql, new 
        { 
            PollId = pollId, 
            UserId = userId, 
            OptionIndex = optionIndex, 
            DigitalSignature = signature, 
            IpAddress = ipAddress 
        });
    }

    public async Task<bool> HasUserVotedAsync(Guid pollId, Guid userId)
    {
        var sql = "SELECT COUNT(1) FROM votes WHERE poll_id = @PollId AND user_id = @UserId AND deleted_at IS NULL";
        var count = await ExecuteScalarAsync<int>(sql, new { PollId = pollId, UserId = userId });
        return count > 0;
    }

    public async Task<Dictionary<int, int>> GetResultsAsync(Guid pollId)
    {
        var sql = @"
            SELECT option_index, COUNT(*) as vote_count
            FROM votes
            WHERE poll_id = @PollId AND deleted_at IS NULL
            GROUP BY option_index";
        var results = await QueryAsync<(int OptionIndex, int VoteCount)>(sql, new { PollId = pollId });
        return results.ToDictionary(r => r.OptionIndex, r => r.VoteCount);
    }
}
