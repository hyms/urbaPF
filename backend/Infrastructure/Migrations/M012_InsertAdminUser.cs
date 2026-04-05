using FluentMigrator;
using UrbaPF.Infrastructure.Services;

namespace UrbaPF.Infrastructure.Migrations;

[Migration(20260402012)]
public class M012_InsertAdminUser : Migration
{
    public override void Up()
    {
        var hasher = new PasswordHasher();
        string adminPasswordHash = hasher.Hash("123abc");
        Insert.IntoTable("users").Row(new
        {
            id = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11"),
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
