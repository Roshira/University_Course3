using Npgsql;

namespace ElectivesApp.Infrastructure;

public interface IDbConnectionFactory
{
    NpgsqlConnection CreateConnection();
}

public class PostgresConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public PostgresConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public NpgsqlConnection CreateConnection() => new NpgsqlConnection(_connectionString);
}