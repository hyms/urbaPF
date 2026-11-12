using FluentMigrator;

namespace UrbaPF.Infrastructure.Migrations;

[Migration(20260402007)]
public class M007_CreateRefreshTokensTable : Migration
{
    public override void Up()
    {
        Create.Table("refresh_tokens")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("user_id").AsGuid().NotNullable()
            .WithColumn("token").AsString(500).NotNullable().Unique()
            .WithColumn("expires_at").AsDateTime().NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("revoked_at").AsDateTime().Nullable();

        Create.ForeignKey("fk_refresh_tokens_user")
            .FromTable("refresh_tokens").ForeignColumn("user_id")
            .ToTable("users").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.Index("idx_refresh_tokens_user").OnTable("refresh_tokens").OnColumn("user_id");
        Create.Index("idx_refresh_tokens_token").OnTable("refresh_tokens").OnColumn("token");
    }

    public override void Down()
    {
        Delete.Index("idx_refresh_tokens_token").OnTable("refresh_tokens");
        Delete.Index("idx_refresh_tokens_user").OnTable("refresh_tokens");
        Delete.ForeignKey("fk_refresh_tokens_user").OnTable("refresh_tokens");
        Delete.Table("refresh_tokens");
    }
}
