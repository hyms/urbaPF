using System.Data;

namespace UrbaPF.Infrastructure.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
    string ConnectionString { get; }
}
