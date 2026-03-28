using FluentMigrator;

namespace UrbaPF.Infrastructure.Migrations;

[Migration(20260227003)]
public class M003_InsertAdminUser : Migration
{
    public override void Up()
    {
        string adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("123abc", 11);
        Insert.IntoTable("users").Row(new
        {
            id = "a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11",
            email = "admin@urbapf.com",
            password_hash = adminPasswordHash,
            full_name = "Administrador Principal",
            phone = "+591 70000001",
            role = 4,
            credibility_level = 5,
            status = 1,
            is_validated = true,
            manager_votes = 0
        });
    }

    public override void Down()
    {
        Delete.FromTable("users").Row(new { email = "admin@urbapf.com" });
    }
}
