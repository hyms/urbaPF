using FluentMigrator;

namespace UrbaPF.Infrastructure.Migrations;

[Migration(20260402008)]
public class M008_CreateIncidentsTable : Migration
{
    public override void Up()
    {
        Create.Table("incidents")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("condominium_id").AsGuid().NotNullable().ForeignKey("fk_incidents_condominium", "condominiums", "id")
            .WithColumn("reporter_id").AsGuid().NotNullable().ForeignKey("fk_incidents_reporter", "users", "id")
            .WithColumn("title").AsString(500).NotNullable()
            .WithColumn("description").AsString(int.MaxValue).Nullable()
            .WithColumn("type").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("priority").AsInt32().NotNullable().WithDefaultValue(2)
            .WithColumn("status").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("location").AsCustom("GEOGRAPHY(POINT, 4326)").Nullable()
            .WithColumn("address_reference").AsString(500).Nullable()
            .WithColumn("media").AsCustom("JSONB").Nullable()
            .WithColumn("resolution_notes").AsString(int.MaxValue).Nullable()
            .WithColumn("resolved_at").AsDateTime().Nullable()
            .WithColumn("closed_at").AsDateTime().Nullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.Table("incident_comments")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("incident_id").AsGuid().NotNullable().ForeignKey("fk_incident_comments_incident", "incidents", "id")
            .WithColumn("author_id").AsGuid().NotNullable().ForeignKey("fk_incident_comments_author", "users", "id")
            .WithColumn("content").AsString(int.MaxValue).NotNullable()
            .WithColumn("is_internal").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.Index("idx_incidents_condominium").OnTable("incidents").OnColumn("condominium_id");
        Create.Index("idx_incidents_status").OnTable("incidents").OnColumn("status");
        Create.Index("idx_incidents_priority").OnTable("incidents").OnColumn("priority");
        Create.Index("idx_incidents_location").OnTable("incidents").OnColumn("location");
    }

    public override void Down()
    {
        Delete.Table("incident_comments");
        Delete.Table("incidents");
    }
}
