using FluentMigrator;
using System;
using BCrypt.Net;

namespace UrbaPF.Infrastructure.Migrations;

[Migration(20260227004)]
public class M004_InsertAdminUser : Migration
{
    public override void Up()
    {
        // Hash password for admin user using application logic
        string adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("123abc", 11);

        Insert.IntoTable("users").Row(new
        {
            id = new Guid("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11"),
            email = "admin@urbapf.local",
            password_hash = adminPasswordHash,
            full_name = "Admin User",
            role = 4, // Admin
            is_validated = true,
            credibility_level = 5,
            status = 1 // Active
        });
    }

    public override void Down()
    {
        Delete.FromTable("users").Row(new { email = "admin@urbapf.local" });
    }
}
