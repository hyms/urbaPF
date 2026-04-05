using Dapper;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Data;
using UrbaPF.Infrastructure.Interfaces;

namespace UrbaPF.Infrastructure.Repositories;

public class UserRepository : BaseRepository, IUserRepository
{
    public UserRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var sql = @"
            SELECT id, email, full_name as FullName, phone, role, credibility_level as CredibilityLevel,
                   status, condominium_id as CondominiumId, lot_number as LotNumber,
                   street_address as StreetAddress, photo_url as PhotoUrl, created_at as CreatedAt, last_login_at as LastLoginAt,
                   is_validated as IsValidated, manager_votes as ManagerVotes
            FROM users 
            WHERE deleted_at IS NULL
            ORDER BY created_at DESC";
        return await QueryAsync<UserDto>(sql);
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var sql = @"
            SELECT id, email, full_name as FullName, phone, role, credibility_level as CredibilityLevel,
                   status, condominium_id as CondominiumId, lot_number as LotNumber,
                   street_address as StreetAddress, photo_url as PhotoUrl, created_at as CreatedAt, last_login_at as LastLoginAt,
                   is_validated as IsValidated, manager_votes as ManagerVotes
            FROM users 
            WHERE id = @Id AND deleted_at IS NULL";
        return await QueryFirstOrDefaultAsync<UserDto>(sql, new { Id = id });
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        var sql = @"
            SELECT id, email, full_name as FullName, phone, role, credibility_level as CredibilityLevel,
                   status, condominium_id as CondominiumId, lot_number as LotNumber,
                   street_address as StreetAddress, photo_url as PhotoUrl, created_at as CreatedAt, last_login_at as LastLoginAt,
                   is_validated as IsValidated, manager_votes as ManagerVotes
            FROM users 
            WHERE email = @Email AND deleted_at IS NULL";
        return await QueryFirstOrDefaultAsync<UserDto>(sql, new { Email = email });
    }

    public async Task<UserDto?> GetByEmailWithPasswordAsync(string email)
    {
        var sql = @"
            SELECT id, email, password_hash as PasswordHash, full_name as FullName, phone, role, 
                   credibility_level as CredibilityLevel, status, condominium_id as CondominiumId, 
                   lot_number as LotNumber, street_address as StreetAddress, created_at as CreatedAt, 
                   last_login_at as LastLoginAt, is_validated as IsValidated, manager_votes as ManagerVotes
            FROM users 
            WHERE email = @Email AND deleted_at IS NULL";
        return await QueryFirstOrDefaultAsync<UserDto>(sql, new { Email = email });
    }

    public async Task<Guid> CreateAsync(CreateUserDto dto, string passwordHash, int role = 2)
    {
        var sql = @"
            INSERT INTO users (id, email, password_hash, full_name, phone, role, credibility_level, status,
                               condominium_id, lot_number, street_address, photo_url, is_validated, manager_votes)
            VALUES (gen_random_uuid(), @Email, @PasswordHash, @FullName, @Phone, @Role, 1, 1, @CondominiumId, @LotNumber, 
                    @StreetAddress, @PhotoUrl, false, 0)
            RETURNING id";
        
        return await ExecuteScalarAsync<Guid>(sql, new 
        { 
            dto.Email, 
            PasswordHash = passwordHash, 
            dto.FullName, 
            dto.Phone, 
            Role = role,
            dto.CondominiumId, 
            dto.LotNumber, 
            dto.StreetAddress,
            dto.PhotoUrl
        });
    }

    public async Task UpdateAsync(Guid id, UpdateUserDto dto)
    {
        var sql = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        if (!string.IsNullOrEmpty(dto.FullName))
        {
            sql.Add("full_name = @FullName");
            parameters.Add("FullName", dto.FullName);
        }
        if (!string.IsNullOrEmpty(dto.Phone))
        {
            sql.Add("phone = @Phone");
            parameters.Add("Phone", dto.Phone);
        }
        if (dto.Role.HasValue)
        {
            sql.Add("role = @Role");
            parameters.Add("Role", dto.Role);
        }
        if (dto.CredibilityLevel.HasValue)
        {
            sql.Add("credibility_level = @CredibilityLevel");
            parameters.Add("CredibilityLevel", dto.CredibilityLevel);
        }
        if (dto.CondominiumId.HasValue)
        {
            sql.Add("condominium_id = @CondominiumId");
            parameters.Add("CondominiumId", dto.CondominiumId);
        }
        if (!string.IsNullOrEmpty(dto.LotNumber))
        {
            sql.Add("lot_number = @LotNumber");
            parameters.Add("LotNumber", dto.LotNumber);
        }
        if (!string.IsNullOrEmpty(dto.StreetAddress))
        {
            sql.Add("street_address = @StreetAddress");
            parameters.Add("StreetAddress", dto.StreetAddress);
        }
        if (!string.IsNullOrEmpty(dto.PhotoUrl))
        {
            sql.Add("photo_url = @PhotoUrl");
            parameters.Add("PhotoUrl", dto.PhotoUrl);
        }
        if (!string.IsNullOrEmpty(dto.FcmToken))
        {
            sql.Add("fcm_token = @FcmToken");
            parameters.Add("FcmToken", dto.FcmToken);
        }

        if (sql.Count == 0) return;

        sql.Add("updated_at = CURRENT_TIMESTAMP");

        var updateSql = $"UPDATE users SET {string.Join(", ", sql)} WHERE id = @Id AND deleted_at IS NULL";
        await ExecuteAsync(updateSql, parameters);
    }

    public async Task SoftDeleteAsync(Guid id)
    {
        var sql = @"
            UPDATE users 
            SET deleted_at = CURRENT_TIMESTAMP 
            WHERE id = @Id";
        await ExecuteAsync(sql, new { Id = id });
    }

    public async Task UpdateLastLoginAsync(Guid id)
    {
        var sql = "UPDATE users SET last_login_at = CURRENT_TIMESTAMP WHERE id = @Id";
        await ExecuteAsync(sql, new { Id = id });
    }

    public async Task UpdatePasswordHashAsync(Guid id, string passwordHash)
    {
        var sql = "UPDATE users SET password_hash = @PasswordHash, updated_at = CURRENT_TIMESTAMP WHERE id = @Id AND deleted_at IS NULL";
        await ExecuteAsync(sql, new { Id = id, PasswordHash = passwordHash });
    }

    public async Task UpdateUserPhotoUrlAsync(Guid userId, string photoUrl)
    {
        const string sql = "UPDATE users SET photo_url = @PhotoUrl, updated_at = CURRENT_TIMESTAMP WHERE id = @Id AND deleted_at IS NULL";
        await ExecuteAsync(sql, new { PhotoUrl = photoUrl, Id = userId });
    }

    public async Task<IEnumerable<UserDto>> GetByCondominiumAsync(Guid condominiumId)
    {
        var sql = @"
            SELECT id, email, full_name as FullName, phone, role, credibility_level as CredibilityLevel,
                   status, condominium_id as CondominiumId, lot_number as LotNumber,
                   street_address as StreetAddress, photo_url as PhotoUrl, created_at as CreatedAt, last_login_at as LastLoginAt,
                   is_validated as IsValidated, manager_votes as ManagerVotes, fcm_token as FcmToken
            FROM users 
            WHERE condominium_id = @CondominiumId AND deleted_at IS NULL
            ORDER BY created_at DESC";
        return await QueryAsync<UserDto>(sql, new { CondominiumId = condominiumId });
    }

    public async Task UpdateFcmTokenAsync(Guid userId, string? fcmToken)
    {
        var sql = "UPDATE users SET fcm_token = @FcmToken, updated_at = CURRENT_TIMESTAMP WHERE id = @Id AND deleted_at IS NULL";
        await ExecuteAsync(sql, new { Id = userId, FcmToken = fcmToken });
    }
}

