using FluentMigrator;

namespace UrbaPF.Infrastructure.Migrations;

[Migration(20260402005)]
public class M005_CreatePollsTable : Migration
{
    public override void Up()
    {
        Create.Table("polls")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("condominium_id").AsGuid().NotNullable().ForeignKey("fk_polls_condominium", "condominiums", "id")
            .WithColumn("created_by_id").AsGuid().NotNullable().ForeignKey("fk_polls_creator", "users", "id")
            .WithColumn("title").AsString(500).NotNullable()
            .WithColumn("description").AsString(int.MaxValue).Nullable()
            .WithColumn("options").AsCustom("JSONB").NotNullable()
            .WithColumn("poll_type").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("starts_at").AsDateTime().NotNullable()
            .WithColumn("ends_at").AsDateTime().NotNullable()
            .WithColumn("requires_justification").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("min_role_to_vote").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("server_secret").AsString(255).NotNullable()
            .WithColumn("status").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();
        
        Create.Index("idx_polls_condominium").OnTable("polls").OnColumn("condominium_id");
    }

    public override void Down()
    {
        Delete.Table("polls");
    }
}
