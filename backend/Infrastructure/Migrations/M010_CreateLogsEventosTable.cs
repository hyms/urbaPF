using FluentMigrator;

namespace UrbaPF.Infrastructure.Migrations;

[Migration(20260402010)]
public class M010_CreateLogsEventosTable : Migration
{
    public override void Up()
    {
        Create.Table("logs_eventos")
            .WithColumn("id").AsInt64().NotNullable().PrimaryKey().Identity()
            .WithColumn("uuid").AsGuid().NotNullable().Unique().WithDefault(SystemMethods.NewGuid)
            .WithColumn("fecha").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentDateTimeOffset)
            .WithColumn("usuario_id").AsGuid().NotNullable()
            .WithColumn("comunidad_id").AsGuid().NotNullable()
            .WithColumn("tipo_evento").AsString(100).NotNullable()
            .WithColumn("entidad_id").AsGuid().NotNullable()
            .WithColumn("data_json").AsCustom("JSONB").NotNullable()
            .WithColumn("hash_verificacion").AsString(int.MaxValue).Nullable();
    }

    public override void Down()
    {
        Delete.Table("logs_eventos");
    }
}
