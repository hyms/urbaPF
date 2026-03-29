using FluentMigrator;

namespace UrbaPF.Infrastructure.Migrations;

[Migration(20260227001)]
public class M001_CreateAllTables : Migration
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

        Create.Table("posts")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("condominium_id").AsGuid().NotNullable().ForeignKey("fk_posts_condominium", "condominiums", "id")
            .WithColumn("author_id").AsGuid().NotNullable().ForeignKey("fk_posts_author", "users", "id")
            .WithColumn("title").AsString(500).NotNullable()
            .WithColumn("content").AsString(int.MaxValue).NotNullable()
            .WithColumn("category").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("is_pinned").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("is_announcement").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("status").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("view_count").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.Table("comments")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("post_id").AsGuid().NotNullable().ForeignKey("fk_comments_post", "posts", "id")
            .WithColumn("author_id").AsGuid().NotNullable().ForeignKey("fk_comments_author", "users", "id")
            .WithColumn("parent_comment_id").AsGuid().Nullable().ForeignKey("fk_comments_parent", "comments", "id")
            .WithColumn("content").AsString(int.MaxValue).NotNullable()
            .WithColumn("credibility_level").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("is_hidden").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("deleted_at").AsDateTime().Nullable();

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

        Create.Table("votes")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("poll_id").AsGuid().NotNullable().ForeignKey("fk_votes_poll", "polls", "id")
            .WithColumn("user_id").AsGuid().NotNullable().ForeignKey("fk_votes_user", "users", "id")
            .WithColumn("option_index").AsInt32().NotNullable()
            .WithColumn("digital_signature").AsString(255).NotNullable()
            .WithColumn("voted_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("ip_address").AsString(50).Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.Index("idx_users_email").OnTable("users").OnColumn("email");
        Create.Index("idx_users_condominium").OnTable("users").OnColumn("condominium_id");
        Create.Index("idx_posts_condominium").OnTable("posts").OnColumn("condominium_id");
        Create.Index("idx_polls_condominium").OnTable("polls").OnColumn("condominium_id");
        Create.UniqueConstraint("uq_poll_user_vote").OnTable("votes").Columns("poll_id", "user_id");
    }

    public override void Down()
    {
        Delete.Table("votes");
        Delete.Table("polls");
        Delete.Table("comments");
        Delete.Table("posts");
        Delete.Table("users");
        Delete.Table("condominiums");
    }
}
