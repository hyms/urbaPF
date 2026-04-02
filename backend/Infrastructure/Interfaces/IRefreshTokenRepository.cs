using System;
using System.Threading.Tasks;
using UrbaPF.Infrastructure.DTOs;

namespace UrbaPF.Infrastructure.Interfaces;

public interface IRefreshTokenRepository
{
    Task SaveAsync(Guid userId, string token, DateTime expiresAt);
    Task<RefreshTokenDto?> GetByTokenAsync(string token);
    Task RevokeAsync(string token, DateTime revokedAt);
}
