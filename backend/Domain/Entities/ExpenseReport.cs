namespace UrbaPF.Domain.Entities;

public class ExpenseReport
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ComunidadId { get; set; }
    public Guid UsuarioId { get; set; }
    public string Type { get; set; } = string.Empty; // INGRESO, EGRESO
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "BOB";
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReceiptUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}