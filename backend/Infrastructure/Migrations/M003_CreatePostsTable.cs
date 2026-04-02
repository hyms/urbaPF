using FluentMigrator;

namespace UrbaPF.Infrastructure.Migrations;

[Migration(20260402003)]
public class M003_CreatePostsTable : Migration
{
    public override void Up()
    {
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
        
        Create.Index("idx_posts_condominium").OnTable("posts").OnColumn("condominium_id");
    }

    public override void Down()
    {
        Delete.Table("posts");
    }
}
