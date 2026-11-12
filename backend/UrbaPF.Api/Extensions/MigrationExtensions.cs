using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace UrbaPF.Api.Extensions;

public static class MigrationExtensions
{
    public static void RunMigrations(this IApplicationBuilder app)
    {
        System.IO.File.WriteAllText("/app/migration_start.txt", "Started");
        try 
        {
            using var scope = app.ApplicationServices.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }
        catch (Exception ex)
        {
            System.IO.File.WriteAllText("/app/migration_error.txt", $"Migration failed: {ex.Message}\n{ex.StackTrace}");
            throw; // Rethrow to stop the app if needed
        }
    }
}
