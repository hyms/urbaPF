using System.Text.Json;
using UrbaPF.Domain.Common;
using UrbaPF.Domain.Entities;
using UrbaPF.Infrastructure.DTOs;
using UrbaPF.Infrastructure.Interfaces;

namespace UrbaPF.Infrastructure.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IAuditRepository _auditRepository;

    public ExpenseService(IExpenseRepository expenseRepository, IAuditRepository auditRepository)
    {
        _expenseRepository = expenseRepository;
        _auditRepository = auditRepository;
    }

    public async Task<Result<IEnumerable<ExpenseDto>>> GetExpensesAsync(Guid comunidadId, int limit = 50, int offset = 0)
    {
        var expenses = await _expenseRepository.GetByComunidadAsync(comunidadId, limit, offset);
        return Result<IEnumerable<ExpenseDto>>.Success(expenses);
    }

    public async Task<Result<ExpenseDto>> GetExpenseByIdAsync(Guid id)
    {
        var expense = await _expenseRepository.GetByIdAsync(id);
        return expense != null 
            ? Result<ExpenseDto>.Success(expense) 
            : Result<ExpenseDto>.Failure("Gasto no encontrado");
    }

    public async Task<Result<Guid>> CreateExpenseAsync(Guid comunidadId, Guid usuarioId, CreateExpenseDto dto)
    {
        var expense = new ExpenseDto
        {
            Id = Guid.NewGuid(),
            ComunidadId = comunidadId,
            UsuarioId = usuarioId,
            Type = dto.Type,
            Category = dto.Category,
            Amount = dto.Amount,
            Currency = dto.Currency,
            Date = dto.Date,
            Description = dto.Description,
            ReceiptUrl = dto.ReceiptUrl
        };

        var id = await _expenseRepository.CreateAsync(expense);

        // Audit Log
        await _auditRepository.CreateAsync(new AuditLog
        {
            Uuid = Guid.NewGuid(),
            Fecha = DateTimeOffset.UtcNow,
            UsuarioId = usuarioId,
            ComunidadId = comunidadId,
            TipoEvento = expense.Type == "INGRESO" ? "INGRESO_REGISTRADO" : "EGRESO_REGISTRADO",
            EntidadId = id,
            DataJson = JsonSerializer.Serialize(dto)
        });

        return Result<Guid>.Success(id);
    }

    public async Task<Result> UpdateExpenseAsync(Guid id, Guid usuarioId, CreateExpenseDto dto)
    {
        var existing = await _expenseRepository.GetByIdAsync(id);
        if (existing == null) return Result.Failure("Gasto no encontrado");

        await _expenseRepository.UpdateAsync(id, dto);

        // Audit Log
        await _auditRepository.CreateAsync(new AuditLog
        {
            Uuid = Guid.NewGuid(),
            Fecha = DateTimeOffset.UtcNow,
            UsuarioId = usuarioId,
            ComunidadId = existing.ComunidadId,
            TipoEvento = "GASTO_ACTUALIZADO",
            EntidadId = id,
            DataJson = JsonSerializer.Serialize(dto)
        });

        return Result.Success();
    }

    public async Task<Result> DeleteExpenseAsync(Guid id, Guid usuarioId)
    {
        var existing = await _expenseRepository.GetByIdAsync(id);
        if (existing == null) return Result.Failure("Gasto no encontrado");

        await _expenseRepository.SoftDeleteAsync(id);

        // Audit Log
        await _auditRepository.CreateAsync(new AuditLog
        {
            Uuid = Guid.NewGuid(),
            Fecha = DateTimeOffset.UtcNow,
            UsuarioId = usuarioId,
            ComunidadId = existing.ComunidadId,
            TipoEvento = "GASTO_ELIMINADO",
            EntidadId = id,
            DataJson = JsonSerializer.Serialize(new { id })
        });

        return Result.Success();
    }

    public async Task<Result<ExpenseSummaryDto>> GetExpenseSummaryAsync(Guid comunidadId)
    {
        var summary = await _expenseRepository.GetSummaryAsync(comunidadId);
        return Result<ExpenseSummaryDto>.Success(summary);
    }
}