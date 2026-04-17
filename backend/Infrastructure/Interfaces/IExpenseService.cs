using UrbaPF.Domain.Common;
using UrbaPF.Infrastructure.DTOs;

namespace UrbaPF.Infrastructure.Interfaces;

public interface IExpenseService
{
    Task<Result<IEnumerable<ExpenseDto>>> GetExpensesAsync(Guid comunidadId, int limit = 50, int offset = 0);
    Task<Result<ExpenseDto>> GetExpenseByIdAsync(Guid id);
    Task<Result<Guid>> CreateExpenseAsync(Guid comunidadId, Guid usuarioId, CreateExpenseDto dto);
    Task<Result> UpdateExpenseAsync(Guid id, Guid usuarioId, CreateExpenseDto dto);
    Task<Result> DeleteExpenseAsync(Guid id, Guid usuarioId);
    Task<Result<ExpenseSummaryDto>> GetExpenseSummaryAsync(Guid comunidadId);
}