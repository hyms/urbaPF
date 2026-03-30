using FluentMigrator;

namespace UrbaPF.Infrastructure.Migrations;

[Migration(20260330001)]
public class M006_CreateAlertsTable : Migration
{
    public override void Up()
    {
        Create.Table("alerts")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("condominium_id").AsGuid().NotNullable().ForeignKey("fk_alerts_condominium", "condominiums", "id")
            .WithColumn("creator_id").AsGuid().NotNullable().ForeignKey("fk_alerts_creator", "users", "id")
            .WithColumn("type").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("title").AsString(500).NotNullable()
            .WithColumn("description").AsString(int.MaxValue).Nullable()
            .WithColumn("location").AsCustom("GEOGRAPHY(POINT, 4326)").Nullable()
            .WithColumn("status").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("reputation_level").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("needs_approval").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("approved_by_id").AsGuid().Nullable().ForeignKey("fk_alerts_approver", "users", "id")
            .WithColumn("approved_at").AsDateTime().Nullable()
            .WithColumn("notified_at").AsDateTime().Nullable()
            .WithColumn("resolved_at").AsDateTime().Nullable()
            .WithColumn("acknowledged_by_id").AsGuid().Nullable().ForeignKey("fk_alerts_ack", "users", "id")
            .WithColumn("acknowledged_at").AsDateTime().Nullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.Table("alert_notifications")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("alert_id").AsGuid().NotNullable().ForeignKey("fk_alert_notif_alert", "alerts", "id")
            .WithColumn("user_id").AsGuid().NotNullable().ForeignKey("fk_alert_notif_user", "users", "id")
            .WithColumn("is_read").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("read_at").AsDateTime().Nullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

        Create.Index("idx_alerts_condominium").OnTable("alerts").OnColumn("condominium_id");
        Create.Index("idx_alerts_status").OnTable("alerts").OnColumn("status");
        Create.Index("idx_alerts_type").OnTable("alerts").OnColumn("type");
        Create.Index("idx_alert_notifications_alert").OnTable("alert_notifications").OnColumn("alert_id");
        Create.Index("idx_alert_notifications_user").OnTable("alert_notifications").OnColumn("user_id");
    }

    public override void Down()
    {
        Delete.Table("alert_notifications");
        Delete.Table("alerts");
    }
}
