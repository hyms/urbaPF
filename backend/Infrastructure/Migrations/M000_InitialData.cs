using FluentMigrator;
using System;

namespace UrbaPF.Infrastructure.Migrations;

[Migration(20260227000)]
public class M000_InitialData : Migration
{
    public override void Up()
    {
        // Insert initial Condominium for testing/development
        Insert.IntoTable("condominiums").Row(new {
            id = new Guid("467b6f3c-502a-43f1-b94a-8d1e3a6c2c9d"),
            name = "Condominium Central",
            address = "Av. Principal #123",
            logo_url = "https://example.com/logo.png",
            description = "Condominium for testing purposes",
            rules = "No loud noises after 10 PM",
            monthly_fee = 100.00m,
            currency = "BOB",
            is_active = true
        });
    }

    public override void Down()
    {
        Delete.FromTable("condominiums").Row(new { id = new Guid("467b6f3c-502a-43f1-b94a-8d1e3a6c2c9d") });
    }
}
