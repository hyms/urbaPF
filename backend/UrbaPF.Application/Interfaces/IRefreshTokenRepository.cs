using System;
using System.Threading.Tasks;
using UrbaPF.Application.DTOs;

namespace UrbaPF.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task SaveAsync(Guid userId, string token, DateTime expiresAt);
    Task<RefreshTokenDto?> GetByTokenAsync(string token);
    Task RevokeAsync(string token, DateTime revokedAt);
}
