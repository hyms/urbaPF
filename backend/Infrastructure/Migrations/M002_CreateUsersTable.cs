using FluentMigrator;

namespace UrbaPF.Infrastructure.Migrations;

[Migration(20260402002)]
public class M002_CreateUsersTable : Migration
{
    public override void Up()
    {
        Create.Table("users")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("email").AsString(255).NotNullable().Unique()
            .WithColumn("password_hash").AsString(255).NotNullable()
            .WithColumn("full_name").AsString(255).NotNullable()
            .WithColumn("phone").AsString(50).Nullable()
            .WithColumn("photo_url").AsString(500).Nullable()
            .WithColumn("role").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("credibility_level").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("status").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("condominium_id").AsGuid().Nullable().ForeignKey("fk_users_condominium", "condominiums", "id")
            .WithColumn("lot_number").AsString(50).Nullable()
            .WithColumn("street_address").AsString(500).Nullable()
            .WithColumn("fcm_token").AsString(500).Nullable()
            .WithColumn("is_validated").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("manager_votes").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("last_login_at").AsDateTime().Nullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();
        
        Create.Index("idx_users_email").OnTable("users").OnColumn("email");
        Create.Index("idx_users_condominium").OnTable("users").OnColumn("condominium_id");
    }

    public override void Down()
    {
        Delete.Table("users");
    }
}
