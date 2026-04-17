using FluentMigrator;

namespace UrbaPF.Infrastructure.Migrations;

[Migration(20260417001)]
public class M015_AddCondominiumCoordinates : Migration
{
    public override void Up()
    {
        Alter.Table("condominiums")
            .AddColumn("latitude").AsDouble().Nullable().WithDefaultValue(-17.607406)
            .AddColumn("longitude").AsDouble().Nullable().WithDefaultValue(-63.097274);
    }

    public override void Down()
    {
        Delete.Column("latitude").FromTable("condominiums");
        Delete.Column("longitude").FromTable("condominiums");
    }
}