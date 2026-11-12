using Dapper;
using UrbaPF.Infrastructure.Data;

namespace UrbaPF.Infrastructure.Repositories;

public class BaseRepository
{
    protected readonly IDbConnectionFactory _connectionFactory;

    public BaseRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    protected virtual async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<T>(sql, param);
    }

    protected virtual async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<T>(sql, param);
    }

    protected virtual async Task<int> ExecuteAsync(string sql, object? param = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, param);
    }

    protected virtual async Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<T?>(sql, param);
    }

    protected virtual async Task<(IEnumerable<T1>, IEnumerable<T2>)> QueryMultipleAsync<T1, T2>(string sql, object? param = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var reader = await connection.QueryMultipleAsync(sql, param);
        return (reader.Read<T1>(), reader.Read<T2>());
    }
}
