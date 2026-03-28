using FluentMigrator;

namespace UrbaPF.Infrastructure.Migrations;

[Migration(20260227001)]
public class M001_CreateAllTables : Migration
{
    public override void Up()
    {
        Create.Table("users")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("email").AsString(255).NotNullable().Unique()
            .WithColumn("password_hash").AsString(255).NotNullable()
            .WithColumn("full_name").AsString(255).NotNullable()
            .WithColumn("phone").AsString(50).Nullable()
            .WithColumn("role").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("credibility_level").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("status").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("condominium_id").AsGuid().Nullable()
            .WithColumn("lot_number").AsString(50).Nullable()
            .WithColumn("street_address").AsString(500).Nullable()
            .WithColumn("photo_url").AsString(500).Nullable()
            .WithColumn("fcm_token").AsString(500).Nullable()
            .WithColumn("is_validated").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("manager_votes").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("last_login_at").AsDateTime().Nullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

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

        Create.Table("posts")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("condominium_id").AsGuid().NotNullable()
            .WithColumn("user_id").AsGuid().NotNullable()
            .WithColumn("title").AsString(255).NotNullable()
            .WithColumn("content").AsString(int.MaxValue).NotNullable()
            .WithColumn("category").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("is_pinned").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("is_announcement").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("status").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.Table("comments")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("post_id").AsGuid().NotNullable()
            .WithColumn("user_id").AsGuid().NotNullable()
            .WithColumn("author_id").AsGuid().NotNullable()
            .WithColumn("parent_comment_id").AsGuid().Nullable()
            .WithColumn("content").AsString(int.MaxValue).NotNullable()
            .WithColumn("credibility_level").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.Table("incidents")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("condominium_id").AsGuid().NotNullable()
            .WithColumn("user_id").AsGuid().NotNullable()
            .WithColumn("reported_by_id").AsGuid().NotNullable()
            .WithColumn("title").AsString(255).NotNullable()
            .WithColumn("description").AsString(int.MaxValue).NotNullable()
            .WithColumn("type").AsInt32().NotNullable()
            .WithColumn("priority").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("latitude").AsDouble().Nullable()
            .WithColumn("longitude").AsDouble().Nullable()
            .WithColumn("location_description").AsString(500).Nullable()
            .WithColumn("occurred_at").AsDateTime().NotNullable()
            .WithColumn("status").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("resolved_at").AsDateTime().Nullable()
            .WithColumn("resolution_notes").AsString(int.MaxValue).Nullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.Table("polls")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("condominium_id").AsGuid().NotNullable()
            .WithColumn("user_id").AsGuid().NotNullable()
            .WithColumn("created_by_id").AsGuid().NotNullable()
            .WithColumn("title").AsString(255).NotNullable()
            .WithColumn("description").AsString(int.MaxValue).Nullable()
            .WithColumn("options").AsCustom("JSONB").NotNullable()
            .WithColumn("poll_type").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("starts_at").AsDateTime().NotNullable()
            .WithColumn("ends_at").AsDateTime().NotNullable()
            .WithColumn("requires_justification").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("min_role_to_vote").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("server_secret").AsString(255).Nullable()
            .WithColumn("status").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.Table("votes")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("poll_id").AsGuid().NotNullable()
            .WithColumn("user_id").AsGuid().NotNullable()
            .WithColumn("option_index").AsInt32().NotNullable()
            .WithColumn("justification").AsString(int.MaxValue).Nullable()
            .WithColumn("digital_signature").AsString(255).Nullable()
            .WithColumn("ip_address").AsString(50).Nullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.Table("alerts")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("condominium_id").AsGuid().NotNullable()
            .WithColumn("created_by_id").AsGuid().NotNullable()
            .WithColumn("alert_type").AsInt32().NotNullable()
            .WithColumn("message").AsString(int.MaxValue).NotNullable()
            .WithColumn("latitude").AsDouble().Nullable()
            .WithColumn("longitude").AsDouble().Nullable()
            .WithColumn("destination_address").AsString(500).Nullable()
            .WithColumn("estimated_arrival").AsDateTime().Nullable()
            .WithColumn("status").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("acknowledged_at").AsDateTime().Nullable()
            .WithColumn("arrived_at").AsDateTime().Nullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.Table("expenses")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("condominium_id").AsGuid().NotNullable()
            .WithColumn("description").AsString(500).NotNullable()
            .WithColumn("amount").AsDecimal(18, 2).NotNullable()
            .WithColumn("category").AsString(100).NotNullable()
            .WithColumn("due_date").AsDateTime().NotNullable()
            .WithColumn("paid_at").AsDateTime().Nullable()
            .WithColumn("paid_by_id").AsGuid().Nullable()
            .WithColumn("receipt_url").AsString(500).Nullable()
            .WithColumn("status").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.ForeignKey("fk_users_condominium")
            .FromTable("users").ForeignColumn("condominium_id")
            .ToTable("condominiums").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.SetNull);

        Create.ForeignKey("fk_posts_condominium")
            .FromTable("posts").ForeignColumn("condominium_id")
            .ToTable("condominiums").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey("fk_posts_user")
            .FromTable("posts").ForeignColumn("user_id")
            .ToTable("users").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey("fk_comments_post")
            .FromTable("comments").ForeignColumn("post_id")
            .ToTable("posts").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey("fk_comments_user")
            .FromTable("comments").ForeignColumn("user_id")
            .ToTable("users").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey("fk_comments_author")
            .FromTable("comments").ForeignColumn("author_id")
            .ToTable("users").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey("fk_comments_parent")
            .FromTable("comments").ForeignColumn("parent_comment_id")
            .ToTable("comments").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey("fk_incidents_condominium")
            .FromTable("incidents").ForeignColumn("condominium_id")
            .ToTable("condominiums").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey("fk_incidents_user")
            .FromTable("incidents").ForeignColumn("user_id")
            .ToTable("users").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey("fk_incidents_reported_by")
            .FromTable("incidents").ForeignColumn("reported_by_id")
            .ToTable("users").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey("fk_polls_condominium")
            .FromTable("polls").ForeignColumn("condominium_id")
            .ToTable("condominiums").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey("fk_polls_user")
            .FromTable("polls").ForeignColumn("user_id")
            .ToTable("users").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey("fk_polls_created_by")
            .FromTable("polls").ForeignColumn("created_by_id")
            .ToTable("users").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey("fk_votes_poll")
            .FromTable("votes").ForeignColumn("poll_id")
            .ToTable("polls").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey("fk_votes_user")
            .FromTable("votes").ForeignColumn("user_id")
            .ToTable("users").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey("fk_alerts_condominium")
            .FromTable("alerts").ForeignColumn("condominium_id")
            .ToTable("condominiums").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey("fk_alerts_created_by")
            .FromTable("alerts").ForeignColumn("created_by_id")
            .ToTable("users").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey("fk_expenses_condominium")
            .FromTable("expenses").ForeignColumn("condominium_id")
            .ToTable("condominiums").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey("fk_expenses_paid_by")
            .FromTable("expenses").ForeignColumn("paid_by_id")
            .ToTable("users").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.SetNull);

        Create.Index("idx_users_email").OnTable("users").OnColumn("email");
        Create.Index("idx_users_condominium").OnTable("users").OnColumn("condominium_id");
        Create.Index("idx_posts_condominium").OnTable("posts").OnColumn("condominium_id");
        Create.Index("idx_posts_user").OnTable("posts").OnColumn("user_id");
        Create.Index("idx_comments_post").OnTable("comments").OnColumn("post_id");
        Create.Index("idx_comments_user").OnTable("comments").OnColumn("user_id");
        Create.Index("idx_incidents_condominium").OnTable("incidents").OnColumn("condominium_id");
        Create.Index("idx_incidents_status").OnTable("incidents").OnColumn("status");
        Create.Index("idx_polls_condominium").OnTable("polls").OnColumn("condominium_id");
        Create.Index("idx_votes_poll").OnTable("votes").OnColumn("poll_id");
        Create.Index("idx_votes_user").OnTable("votes").OnColumn("user_id");
        Create.Index("idx_alerts_condominium").OnTable("alerts").OnColumn("condominium_id");
        Create.Index("idx_alerts_status").OnTable("alerts").OnColumn("status");
        Create.Index("idx_expenses_condominium").OnTable("expenses").OnColumn("condominium_id");
        Create.Index("idx_expenses_status").OnTable("expenses").OnColumn("status");
    }

    public override void Down()
    {
        Delete.Index("idx_expenses_status").OnTable("expenses");
        Delete.Index("idx_expenses_condominium").OnTable("expenses");
        Delete.Index("idx_alerts_status").OnTable("alerts");
        Delete.Index("idx_alerts_condominium").OnTable("alerts");
        Delete.Index("idx_votes_user").OnTable("votes");
        Delete.Index("idx_votes_poll").OnTable("votes");
        Delete.Index("idx_polls_condominium").OnTable("polls");
        Delete.Index("idx_incidents_status").OnTable("incidents");
        Delete.Index("idx_incidents_condominium").OnTable("incidents");
        Delete.Index("idx_comments_user").OnTable("comments");
        Delete.Index("idx_comments_post").OnTable("comments");
        Delete.Index("idx_posts_user").OnTable("posts");
        Delete.Index("idx_posts_condominium").OnTable("posts");
        Delete.Index("idx_users_condominium").OnTable("users");
        Delete.Index("idx_users_email").OnTable("users");

        Delete.ForeignKey("fk_expenses_paid_by").OnTable("expenses");
        Delete.ForeignKey("fk_expenses_condominium").OnTable("expenses");
        Delete.ForeignKey("fk_alerts_created_by").OnTable("alerts");
        Delete.ForeignKey("fk_alerts_condominium").OnTable("alerts");
        Delete.ForeignKey("fk_votes_user").OnTable("votes");
        Delete.ForeignKey("fk_votes_poll").OnTable("votes");
        Delete.ForeignKey("fk_polls_created_by").OnTable("polls");
        Delete.ForeignKey("fk_polls_user").OnTable("polls");
        Delete.ForeignKey("fk_polls_condominium").OnTable("polls");
        Delete.ForeignKey("fk_incidents_reported_by").OnTable("incidents");
        Delete.ForeignKey("fk_incidents_user").OnTable("incidents");
        Delete.ForeignKey("fk_incidents_condominium").OnTable("incidents");
        Delete.ForeignKey("fk_comments_parent").OnTable("comments");
        Delete.ForeignKey("fk_comments_author").OnTable("comments");
        Delete.ForeignKey("fk_comments_user").OnTable("comments");
        Delete.ForeignKey("fk_comments_post").OnTable("comments");
        Delete.ForeignKey("fk_posts_user").OnTable("posts");
        Delete.ForeignKey("fk_posts_condominium").OnTable("posts");
        Delete.ForeignKey("fk_users_condominium").OnTable("users");

        Delete.Table("expenses");
        Delete.Table("alerts");
        Delete.Table("votes");
        Delete.Table("polls");
        Delete.Table("incidents");
        Delete.Table("comments");
        Delete.Table("posts");
        Delete.Table("condominiums");
        Delete.Table("users");
    }
}
