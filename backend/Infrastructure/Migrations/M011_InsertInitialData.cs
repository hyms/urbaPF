using FluentMigrator;

namespace UrbaPF.Infrastructure.Migrations;

[Migration(20260402011)]
public class M011_InsertInitialData : Migration
{
    public override void Up()
    {
        Insert.IntoTable("condominiums").Row(new
        {
            id = Guid.Parse("467b6f3c-502a-43f1-b94a-8d1e3a6c2c9d"),
            name = "Condominium Central",
            address = "Av. Principal #123, Santa Cruz, Bolivia",
            logo_url = "https://example.com/logo.png",
            description = "Condominio principal para pruebas y desarrollo",
            rules = "1. Respetar horas de silencio después de las 22:00\n2. Mantener áreas comunes limpias\n3. No realizar construcciones sin autorización",
            monthly_fee = 100.00m,
            currency = "BOB",
            is_active = true
        });
    }

    public override void Down()
    {
        Delete.FromTable("condominiums").Row(new { id = "467b6f3c-502a-43f1-b94a-8d1e3a6c2c9d" });
    }
}
