namespace UrbaPF.Infrastructure.DTOs;

public class CondominiumDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public string? Rules { get; set; }
    public decimal MonthlyFee { get; set; }
    public string Currency { get; set; } = "BOB";
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class CreateCondominiumDto
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Rules { get; set; }
    public decimal MonthlyFee { get; set; }
    public string? Currency { get; set; }
}

public class UpdateCondominiumDto
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public string? Rules { get; set; }
    public decimal? MonthlyFee { get; set; }
    public bool? IsActive { get; set; }
}
