using FluentMigrator;

namespace UrbaPF.Infrastructure.Migrations;

[Migration(20260402006)]
public class M006_CreateVotesTable : Migration
{
    public override void Up()
    {
        Create.Table("votes")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("poll_id").AsGuid().NotNullable().ForeignKey("fk_votes_poll", "polls", "id")
            .WithColumn("user_id").AsGuid().NotNullable().ForeignKey("fk_votes_user", "users", "id")
            .WithColumn("option_index").AsInt32().NotNullable()
            .WithColumn("digital_signature").AsString(255).NotNullable()
            .WithColumn("voted_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("ip_address").AsString(50).Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();
        
        Create.UniqueConstraint("uq_poll_user_vote").OnTable("votes").Columns("poll_id", "user_id");
    }

    public override void Down()
    {
        Delete.Table("votes");
    }
}
