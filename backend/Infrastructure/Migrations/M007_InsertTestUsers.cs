using FluentMigrator;
using UrbaPF.Infrastructure.Services;

namespace UrbaPF.Infrastructure.Migrations;

[Migration(20260331001)]
public class M007_InsertTestUsers : Migration
{
    public override void Up()
    {
        var hasher = new PasswordHasher();
        
        // Manager/Encargado user - High reputation
        string managerPasswordHash = hasher.Hash("Admin123!");
        Insert.IntoTable("users").Row(new
        {
            id = "b1fc99-9c0b-4ef8-bb6d-6bb9bd380a22",
            email = "encargado@urbapf.com",
            password_hash = managerPasswordHash,
            full_name = "Juan Pérez - Encargado",
            phone = "+591 70000002",
            role = 3,
            credibility_level = 5,
            status = 1,
            is_validated = true,
            manager_votes = 2,
            condominium_id = "c0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11"
        });

        // Neighbor with high reputation - for testing normal flows
        string neighborGoodHash = hasher.Hash("Vecino123!");
        Insert.IntoTable("users").Row(new
        {
            id = "c2edd99-9c0b-4ef8-bb6d-6bb9bd380a33",
            email = "vecino@urbapf.com",
            password_hash = neighborGoodHash,
            full_name = "María García - Vecina",
            phone = "+591 70000003",
            role = 2,
            credibility_level = 5,
            status = 1,
            is_validated = true,
            manager_votes = 3,
            condominium_id = "c0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11",
            lot_number = "A-101"
        });

        // Neighbor with LOW reputation - for testing emergency approval flow
        string neighborBadHash = hasher.Hash("Vecino123!");
        Insert.IntoTable("users").Row(new
        {
            id = "d3fee99-9c0b-4ef8-bb6d-6bb9bd380a44",
            email = "nuevo@urbapf.com",
            password_hash = neighborBadHash,
            full_name = "Pedro Nuevo - Vecino",
            phone = "+591 70000004",
            role = 2,
            credibility_level = 2,
            status = 1,
            is_validated = true,
            manager_votes = 0,
            condominium_id = "c0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11",
            lot_number = "B-205"
        });
    }

    public override void Down()
    {
        Delete.FromTable("users").Row(new { email = "encargado@urbapf.com" });
        Delete.FromTable("users").Row(new { email = "vecino@urbapf.com" });
        Delete.FromTable("users").Row(new { email = "nuevo@urbapf.com" });
    }
}
