using Dapper;
using UrbaPF.Infrastructure.Data;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;
using UrbaPF.Infrastructure.Repositories;

namespace UrbaPF.Infrastructure.Repositories;

public class RefreshTokenRepository : BaseRepository, IRefreshTokenRepository
{
    public RefreshTokenRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task SaveAsync(Guid userId, string token, DateTime expiresAt)
    {
        var sql = @"
            INSERT INTO refresh_tokens (id, user_id, token, expires_at)
            VALUES (gen_random_uuid(), @UserId, @Token, @ExpiresAt)";
        await ExecuteAsync(sql, new { UserId = userId, Token = token, ExpiresAt = expiresAt });
    }

    public async Task<RefreshTokenDto?> GetByTokenAsync(string token)
    {
        var sql = @"
            SELECT id, user_id as UserId, token, expires_at as ExpiresAt, created_at as CreatedAt, revoked_at as RevokedAt
            FROM refresh_tokens 
            WHERE token = @Token";
        return await QueryFirstOrDefaultAsync<RefreshTokenDto>(sql, new { Token = token });
    }

    public async Task RevokeAsync(string token, DateTime revokedAt)
    {
        var sql = "UPDATE refresh_tokens SET revoked_at = @RevokedAt WHERE token = @Token AND revoked_at IS NULL";
        await ExecuteAsync(sql, new { Token = token, RevokedAt = revokedAt });
    }
}
