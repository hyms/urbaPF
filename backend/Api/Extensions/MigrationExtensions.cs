using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace UrbaPF.Api.Extensions;

public static class MigrationExtensions
{
    public static void RunMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }
}
