namespace UrbaPF.Infrastructure.DTOs;

public class ExpenseDto
{
    public Guid Id { get; set; }
    public Guid ComunidadId { get; set; }
    public Guid UsuarioId { get; set; }
    public string UsuarioName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "BOB";
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReceiptUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateExpenseDto
{
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "BOB";
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReceiptUrl { get; set; }
}

public class ExpenseSummaryDto
{
    public decimal Balance { get; set; }
    public decimal TotalMonthlyExpenses { get; set; }
    public IEnumerable<ExpenseDto> TopExpenses { get; set; } = new List<ExpenseDto>();
}