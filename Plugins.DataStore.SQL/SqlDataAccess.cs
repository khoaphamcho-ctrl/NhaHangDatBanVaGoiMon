using System.Data;
using Microsoft.Data.SqlClient;

namespace Plugins.DataStore.SQL;

public class SqlDataAccess
{
    private readonly string _connectionString;

    public SqlDataAccess(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
