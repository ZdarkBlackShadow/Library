using MySql.Data.MySqlClient;
using System.Data;

namespace Library.Core.Data;

public class DatabaseContext
{
    private readonly string _connectionString = "Server=127.0.0.1;Port=3306;Database=library_db;Uid=library_user;Pwd=Pa55word!;CharSet=utf8mb4;AllowZeroDateTime=True;";

    public MySqlConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();
            return connection.State == ConnectionState.Open;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error when connecting to database: {e.Message}");
            return false;
        }
    }
}
