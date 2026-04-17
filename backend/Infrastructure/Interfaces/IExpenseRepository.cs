using UrbaPF.Infrastructure.DTOs;

namespace UrbaPF.Infrastructure.Interfaces;

public interface IExpenseRepository
{
    Task<IEnumerable<ExpenseDto>> GetByComunidadAsync(Guid comunidadId, int limit = 50, int offset = 0);
    Task<ExpenseDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(ExpenseDto expense);
    Task UpdateAsync(Guid id, CreateExpenseDto dto);
    Task SoftDeleteAsync(Guid id);
    Task<ExpenseSummaryDto> GetSummaryAsync(Guid comunidadId);
}