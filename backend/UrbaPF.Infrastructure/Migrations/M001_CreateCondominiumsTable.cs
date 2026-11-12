using FluentMigrator;

namespace UrbaPF.Infrastructure.Migrations;

[Migration(20260402001)]
public class M001_CreateCondominiumsTable : Migration
{
    public override void Up()
    {
        Create.Table("condominiums")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("address").AsString(500).NotNullable()
            .WithColumn("logo_url").AsString(500).Nullable()
            .WithColumn("description").AsString(int.MaxValue).Nullable()
            .WithColumn("rules").AsString(int.MaxValue).Nullable()
            .WithColumn("monthly_fee").AsDecimal(18, 2).NotNullable().WithDefaultValue(0)
            .WithColumn("currency").AsString(10).NotNullable().WithDefaultValue("BOB")
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();
    }

    public override void Down()
    {
        Delete.Table("condominiums");
    }
}
