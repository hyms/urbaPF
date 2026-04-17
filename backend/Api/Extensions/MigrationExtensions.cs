using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Dapper;

namespace UrbaPF.Api.Extensions;

public static class MigrationExtensions
{
    public static void RunMigrations(this IApplicationBuilder app)
    {
        var maxRetries = 5;
        var retryDelay = 3;
        
        for (int i = 0; i < maxRetries; i++)
        {
            try 
            {
                using var scope = app.ApplicationServices.CreateScope();
                
                var connectionFactory = scope.ServiceProvider.GetRequiredService<UrbaPF.Infrastructure.Data.IDbConnectionFactory>();
                using var connection = connectionFactory.CreateConnection();
                connection.Open();
                
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS ""VersionInfo"" (
                        ""Version"" bigint NOT NULL,
                        ""AppliedOn"" timestamp NOT NULL,
                        ""Description"" varchar(200) NOT NULL
                    )");
                
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                runner.MigrateUp();
                
                Console.WriteLine("[Migrations] Completed successfully.");
                return;
            }
            catch (Exception ex)
            {
                if (i == maxRetries - 1)
                {
                    Console.WriteLine($"Migration failed after {maxRetries} attempts: {ex.Message}");
                }
                else
                {
                    Console.WriteLine($"Migration attempt {i + 1} failed, retrying in {retryDelay}s...");
                    Thread.Sleep(TimeSpan.FromSeconds(retryDelay));
                }
            }
        }
    }
}
