using FluentMigrator;

namespace UrbaPF.Infrastructure.Migrations;

[Migration(20260402014)]
public class M014_CreateExpensesTable : Migration
{
    public override void Up()
    {
        Create.Table("expense_reports")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("comunidad_id").AsGuid().NotNullable().ForeignKey("fk_expenses_comunidad", "condominiums", "id")
            .WithColumn("usuario_id").AsGuid().NotNullable().ForeignKey("fk_expenses_usuario", "users", "id")
            .WithColumn("type").AsString(20).NotNullable() // INGRESO / EGRESO
            .WithColumn("category").AsString(100).NotNullable()
            .WithColumn("amount").AsDecimal(18, 2).NotNullable()
            .WithColumn("currency").AsString(10).NotNullable().WithDefaultValue("BOB")
            .WithColumn("date").AsDate().NotNullable()
            .WithColumn("description").AsString(int.MaxValue).NotNullable()
            .WithColumn("receipt_url").AsString(500).Nullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.Index("idx_expenses_comunidad").OnTable("expense_reports").OnColumn("comunidad_id");
        Create.Index("idx_expenses_date").OnTable("expense_reports").OnColumn("date");
    }

    public override void Down()
    {
        Delete.Table("expense_reports");
    }
}