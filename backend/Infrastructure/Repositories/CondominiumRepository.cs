using Dapper;
using UrbaPF.Infrastructure.Data;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;

namespace UrbaPF.Infrastructure.Repositories;

public class CondominiumRepository : BaseRepository, ICondominiumRepository
{
    public CondominiumRepository(DbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<IEnumerable<CondominiumDto>> GetAllAsync()
    {
        var sql = "SELECT id, name, address, logo_url as LogoUrl, description, rules, monthly_fee as MonthlyFee, currency, created_at as CreatedAt, is_active as IsActive FROM condominiums WHERE is_deleted = false AND is_active = true ORDER BY name";
        return await QueryAsync<CondominiumDto>(sql);
    }

    public async Task<CondominiumDto?> GetByIdAsync(Guid id)
    {
        var sql = "SELECT id, name, address, logo_url as LogoUrl, description, rules, monthly_fee as MonthlyFee, currency, created_at as CreatedAt, is_active as IsActive FROM condominiums WHERE id = @Id AND is_deleted = false";
        return await QueryFirstOrDefaultAsync<CondominiumDto>(sql, new { Id = id });
    }

    public async Task<Guid> CreateAsync(CreateCondominiumDto dto)
    {
        var sql = @"
            INSERT INTO condominiums (name, address, description, rules, monthly_fee, currency)
            VALUES (@Name, @Address, @Description, @Rules, @MonthlyFee, @Currency)
            RETURNING id";
        return await ExecuteScalarAsync<Guid>(sql, new 
        { 
            dto.Name, 
            dto.Address, 
            dto.Description, 
            dto.Rules, 
            dto.MonthlyFee, 
            dto.Currency 
        });
    }

    public async Task UpdateAsync(Guid id, UpdateCondominiumDto dto)
    {
        var sql = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        if (!string.IsNullOrEmpty(dto.Name))
        {
            sql.Add("name = @Name");
            parameters.Add("Name", dto.Name);
        }
        if (!string.IsNullOrEmpty(dto.Address))
        {
            sql.Add("address = @Address");
            parameters.Add("Address", dto.Address);
        }
        if (dto.Description is not null)
        {
            sql.Add("description = @Description");
            parameters.Add("Description", dto.Description);
        }
        if (dto.Rules is not null)
        {
            sql.Add("rules = @Rules");
            parameters.Add("Rules", dto.Rules);
        }
        if (dto.MonthlyFee.HasValue)
        {
            sql.Add("monthly_fee = @MonthlyFee");
            parameters.Add("MonthlyFee", dto.MonthlyFee);
        }
        if (dto.IsActive.HasValue)
        {
            sql.Add("is_active = @IsActive");
            parameters.Add("IsActive", dto.IsActive);
        }

        if (sql.Count == 0) return;

        sql.Add("updated_at = CURRENT_TIMESTAMP");

        var updateSql = $"UPDATE condominiums SET {string.Join(", ", sql)} WHERE id = @Id AND is_deleted = false";
        await ExecuteAsync(updateSql, parameters);
    }

    public async Task SoftDeleteAsync(Guid id)
    {
        var sql = "UPDATE condominiums SET is_deleted = true, deleted_at = CURRENT_TIMESTAMP WHERE id = @Id";
        await ExecuteAsync(sql, new { Id = id });
    }
}
