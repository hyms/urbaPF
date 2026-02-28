using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace UrbaPF.Infrastructure.Data;

public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        var host = configuration["DB_HOST"] ?? "localhost";
        var port = configuration["DB_PORT"] ?? "5432";
        var database = configuration["DB_NAME"] ?? "urbapf";
        var user = configuration["DB_USER"] ?? "postgres";
        var password = configuration["DB_PASSWORD"] ?? "postgres";

        _connectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password}";
    }

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }

    public string ConnectionString => _connectionString;
}
