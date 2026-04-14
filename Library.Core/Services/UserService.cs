using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Library.Core.Data;
using Library.Core.Models;
using MySql.Data.MySqlClient;

namespace Library.Core.Services;

public class UserService
{
    private readonly DatabaseContext _context = new();

    private const int DegreeOfParallelism = 8;
    private const int MemorySize = 65536;
    private const int Iterations = 4;

    public bool Register(string username, string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = HashPassword(password, salt);

        using var conn = _context.CreateConnection();
        conn.Open();
        
        string query = "INSERT INTO users (username, password_hash, salt) VALUES (@user, @hash, @salt)";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@user", username);
        cmd.Parameters.AddWithValue("@hash", hash);
        cmd.Parameters.AddWithValue("@salt", salt);

        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Login(string username, string password)
    {
        using var conn = _context.CreateConnection();
        conn.Open();

        string query = "SELECT password_hash, salt FROM users WHERE username = @user";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@user", username);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            byte[] storedHash = (byte[])reader["password_hash"];
            byte[] salt = (byte[])reader["salt"];

            byte[] computedHash = HashPassword(password, salt);
            
            return storedHash.SequenceEqual(computedHash);
        }
        return false;
    }

    private byte[] HashPassword(string password, byte[] salt)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));
        argon2.Salt = salt;
        argon2.DegreeOfParallelism = DegreeOfParallelism;
        argon2.MemorySize = MemorySize;
        argon2.Iterations = Iterations;

        return argon2.GetBytes(32);
    }
}
