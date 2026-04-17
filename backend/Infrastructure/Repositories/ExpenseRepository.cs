using Dapper;
using UrbaPF.Infrastructure.Data;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;

namespace UrbaPF.Infrastructure.Repositories;

public class ExpenseRepository : BaseRepository, IExpenseRepository
{
    public ExpenseRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<IEnumerable<ExpenseDto>> GetByComunidadAsync(Guid comunidadId, int limit = 50, int offset = 0)
    {
        var sql = @"
            SELECT e.*, u.full_name as UsuarioName
            FROM expense_reports e
            JOIN users u ON e.usuario_id = u.id
            WHERE e.comunidad_id = @ComunidadId AND e.deleted_at IS NULL
            ORDER BY e.date DESC, e.created_at DESC
            LIMIT @Limit OFFSET @Offset";
        
        return await QueryAsync<ExpenseDto>(sql, new { ComunidadId = comunidadId, Limit = limit, Offset = offset });
    }

    public async Task<ExpenseDto?> GetByIdAsync(Guid id)
    {
        var sql = @"
            SELECT e.*, u.full_name as UsuarioName
            FROM expense_reports e
            JOIN users u ON e.usuario_id = u.id
            WHERE e.id = @Id AND e.deleted_at IS NULL";
        
        return await QueryFirstOrDefaultAsync<ExpenseDto>(sql, new { Id = id });
    }

    public async Task<Guid> CreateAsync(ExpenseDto expense)
    {
        var sql = @"
            INSERT INTO expense_reports (id, comunidad_id, usuario_id, type, category, amount, currency, date, description, receipt_url)
            VALUES (@Id, @ComunidadId, @UsuarioId, @Type, @Category, @Amount, @Currency, @Date, @Description, @ReceiptUrl)
            RETURNING id";
        
        return await ExecuteScalarAsync<Guid>(sql, expense);
    }

    public async Task UpdateAsync(Guid id, CreateExpenseDto dto)
    {
        var sql = @"
            UPDATE expense_reports 
            SET type = @Type, category = @Category, amount = @Amount, currency = @Currency, 
                date = @Date, description = @Description, receipt_url = @ReceiptUrl, 
                updated_at = CURRENT_TIMESTAMP
            WHERE id = @Id AND deleted_at IS NULL";
        
        await ExecuteAsync(sql, new { 
            Id = id, 
            dto.Type, 
            dto.Category, 
            dto.Amount, 
            dto.Currency, 
            dto.Date, 
            dto.Description, 
            dto.ReceiptUrl 
        });
    }

    public async Task SoftDeleteAsync(Guid id)
    {
        var sql = "UPDATE expense_reports SET deleted_at = CURRENT_TIMESTAMP WHERE id = @Id";
        await ExecuteAsync(sql, new { Id = id });
    }

    public async Task<ExpenseSummaryDto> GetSummaryAsync(Guid comunidadId)
    {
        // 1. Balance total
        var sqlBalance = @"
            SELECT 
                SUM(CASE WHEN type = 'INGRESO' THEN amount ELSE -amount END) as Balance
            FROM expense_reports
            WHERE comunidad_id = @ComunidadId AND deleted_at IS NULL";
        
        var balance = await ExecuteScalarAsync<decimal?>(sqlBalance, new { ComunidadId = comunidadId }) ?? 0;

        // 2. Gastos del mes actual
        var sqlMonthly = @"
            SELECT SUM(amount)
            FROM expense_reports
            WHERE comunidad_id = @ComunidadId 
            AND type = 'EGRESO' 
            AND deleted_at IS NULL
            AND date >= DATE_TRUNC('month', CURRENT_DATE)
            AND date < DATE_TRUNC('month', CURRENT_DATE) + INTERVAL '1 month'";
        
        var monthlyExpenses = await ExecuteScalarAsync<decimal?>(sqlMonthly, new { ComunidadId = comunidadId }) ?? 0;

        // 3. Top 3 gastos del mes
        var sqlTop = @"
            SELECT e.*, u.full_name as UsuarioName
            FROM expense_reports e
            JOIN users u ON e.usuario_id = u.id
            WHERE e.comunidad_id = @ComunidadId 
            AND e.type = 'EGRESO' 
            AND e.deleted_at IS NULL
            AND e.date >= DATE_TRUNC('month', CURRENT_DATE)
            ORDER BY e.amount DESC
            LIMIT 3";
        
        var topExpenses = await QueryAsync<ExpenseDto>(sqlTop, new { ComunidadId = comunidadId });

        return new ExpenseSummaryDto
        {
            Balance = balance,
            TotalMonthlyExpenses = monthlyExpenses,
            TopExpenses = topExpenses
        };
    }
}