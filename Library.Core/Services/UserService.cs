using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Library.Core.Data;
using MySql.Data.MySqlClient;

namespace Library.Core.Services;

public class UserService
{
    private readonly DatabaseContext _context = new();

    // OWASP-recommended Argon2id minimum parameters (as of 2024):
    // memory >= 19 MB, iterations >= 2, parallelism >= 1
    // These settings are well above minimums — adjust if login feels slow.
    private const int DegreeOfParallelism = 4;
    private const int MemorySize = 65536; // 64 MB
    private const int Iterations = 4;
    private const int HashLength = 32;    // 256-bit output

    public async Task<bool> Register(string username, string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        // Run CPU-intensive Argon2 on a thread-pool thread so the UI stays responsive
        byte[] hash = await Task.Run(() => HashPassword(password, salt));

        using var conn = _context.CreateConnection();
        await conn.OpenAsync();

        const string query =
            "INSERT INTO users (username, password_hash, salt) VALUES (@user, @hash, @salt)";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@user", username);
        cmd.Parameters.AddWithValue("@hash", hash);
        cmd.Parameters.AddWithValue("@salt", salt);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    /// <summary>
    /// Returns (success, userId). userId is 0 on failure.
    /// </summary>
    public async Task<(bool Success, int UserId)> Login(string username, string password)
    {
        byte[] storedHash;
        byte[] salt;
        int userId;

        // Open connection only long enough to fetch credentials, then close it
        // before running the expensive Argon2 computation
        using (var conn = _context.CreateConnection())
        {
            await conn.OpenAsync();
            const string query =
                "SELECT id, password_hash, salt FROM users WHERE username = @user";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@user", username);
            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return (false, 0);

            userId = reader.GetInt32(reader.GetOrdinal("id"));
            storedHash = (byte[])reader["password_hash"];
            salt = (byte[])reader["salt"];
        }

        // Run on a thread-pool thread — Argon2id takes ~200–500 ms at these settings
        byte[] computedHash = await Task.Run(() => HashPassword(password, salt));

        // FixedTimeEquals is constant-time — prevents timing side-channel attacks
        bool matches = CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
        return matches ? (true, userId) : (false, 0);
    }

    private static byte[] HashPassword(string password, byte[] salt)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));
        argon2.Salt = salt;
        argon2.DegreeOfParallelism = DegreeOfParallelism;
        argon2.MemorySize = MemorySize;
        argon2.Iterations = Iterations;
        return argon2.GetBytes(HashLength);
    }
}
