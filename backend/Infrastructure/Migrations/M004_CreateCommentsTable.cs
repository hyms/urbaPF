using FluentMigrator;

namespace UrbaPF.Infrastructure.Migrations;

[Migration(20260402004)]
public class M004_CreateCommentsTable : Migration
{
    public override void Up()
    {
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
    }

    public override void Down()
    {
        Delete.Table("comments");
    }
}
