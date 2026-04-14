using System.Data.Common;
using Library.Core.Data;
using Library.Core.Models;
using MySql.Data.MySqlClient;

namespace Library.Core.Services;

public class LoanService
{
    private readonly DatabaseContext _db = new();

    public async Task<bool> BorrowBook(int bookId, int userId, int durationDays = 14)
    {
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        using var tx = conn.BeginTransaction();

        try
        {
            // FOR UPDATE locks the row so a concurrent borrow cannot sneak in between
            // the availability check and the status update
            const string checkQuery = "SELECT is_available FROM books WHERE id = @bookId FOR UPDATE";
            using var checkCmd = new MySqlCommand(checkQuery, conn, tx);
            checkCmd.Parameters.AddWithValue("@bookId", bookId);
            var available = await checkCmd.ExecuteScalarAsync();

            if (available == null || !Convert.ToBoolean(available))
            {
                tx.Rollback();
                return false;
            }

            const string loanQuery =
                "INSERT INTO loans (book_id, user_id, borrow_date, due_date) " +
                "VALUES (@bookId, @userId, @borrowDate, @dueDate)";
            using var loanCmd = new MySqlCommand(loanQuery, conn, tx);
            loanCmd.Parameters.AddWithValue("@bookId", bookId);
            loanCmd.Parameters.AddWithValue("@userId", userId);
            loanCmd.Parameters.AddWithValue("@borrowDate", DateTime.Now);
            loanCmd.Parameters.AddWithValue("@dueDate", DateTime.Now.AddDays(durationDays));
            await loanCmd.ExecuteNonQueryAsync();

            const string updateQuery = "UPDATE books SET is_available = FALSE WHERE id = @bookId";
            using var updateCmd = new MySqlCommand(updateQuery, conn, tx);
            updateCmd.Parameters.AddWithValue("@bookId", bookId);
            await updateCmd.ExecuteNonQueryAsync();

            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            return false;
        }
    }

    public async Task<bool> ReturnBook(int loanId)
    {
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        using var tx = conn.BeginTransaction();

        try
        {
            const string getQuery =
                "SELECT book_id FROM loans WHERE id = @loanId AND return_date IS NULL";
            using var getCmd = new MySqlCommand(getQuery, conn, tx);
            getCmd.Parameters.AddWithValue("@loanId", loanId);
            var bookIdObj = await getCmd.ExecuteScalarAsync();

            if (bookIdObj == null)
            {
                tx.Rollback();
                return false;
            }

            int bookId = Convert.ToInt32(bookIdObj);

            const string returnQuery =
                "UPDATE loans SET return_date = @returnDate WHERE id = @loanId";
            using var returnCmd = new MySqlCommand(returnQuery, conn, tx);
            returnCmd.Parameters.AddWithValue("@returnDate", DateTime.Now);
            returnCmd.Parameters.AddWithValue("@loanId", loanId);
            await returnCmd.ExecuteNonQueryAsync();

            const string updateQuery = "UPDATE books SET is_available = TRUE WHERE id = @bookId";
            using var updateCmd = new MySqlCommand(updateQuery, conn, tx);
            updateCmd.Parameters.AddWithValue("@bookId", bookId);
            await updateCmd.ExecuteNonQueryAsync();

            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            return false;
        }
    }

    public async Task<List<Loan>> GetActiveLoans()
    {
        var list = new List<Loan>();
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        const string query =
            "SELECT l.*, b.title AS book_title, u.username FROM loans l " +
            "JOIN books b ON l.book_id = b.id " +
            "JOIN users u ON l.user_id = u.id " +
            "WHERE l.return_date IS NULL ORDER BY l.due_date";
        using var cmd = new MySqlCommand(query, conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add(ReadLoan(reader));
        return list;
    }

    public async Task<List<Loan>> GetAllLoans()
    {
        var list = new List<Loan>();
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        const string query =
            "SELECT l.*, b.title AS book_title, u.username FROM loans l " +
            "JOIN books b ON l.book_id = b.id " +
            "JOIN users u ON l.user_id = u.id " +
            "ORDER BY l.borrow_date DESC";
        using var cmd = new MySqlCommand(query, conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add(ReadLoan(reader));
        return list;
    }

    private static Loan ReadLoan(DbDataReader reader)
    {
        int returnOrdinal = reader.GetOrdinal("return_date");
        return new Loan
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            BookId = reader.GetInt32(reader.GetOrdinal("book_id")),
            UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
            BorrowDate = reader.GetDateTime(reader.GetOrdinal("borrow_date")),
            DueDate = reader.GetDateTime(reader.GetOrdinal("due_date")),
            ReturnDate = reader.IsDBNull(returnOrdinal)
                ? null : reader.GetDateTime(returnOrdinal),
            BookTitle = reader.GetString(reader.GetOrdinal("book_title")),
            Username = reader.GetString(reader.GetOrdinal("username"))
        };
    }
}
