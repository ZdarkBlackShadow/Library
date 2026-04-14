using Library.Core.Data;
using Library.Core.Models;
using MySql.Data.MySqlClient;

namespace Library.Core.Services;

public class LoanService
{
    private readonly DatabaseContext _db = new();

    public bool BorrowBook(int bookId, int userId, int durationDays = 14)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            var checkQuery = "SELECT is_available FROM books WHERE id = @bookId";
            using var checkCmd = new MySqlCommand(checkQuery, conn, tx);
            checkCmd.Parameters.AddWithValue("@bookId", bookId);
            var available = checkCmd.ExecuteScalar();

            if (available == null || !Convert.ToBoolean(available))
                return false;

            var loanQuery = "INSERT INTO loans (book_id, user_id, borrow_date, due_date) VALUES (@bookId, @userId, @borrowDate, @dueDate)";
            using var loanCmd = new MySqlCommand(loanQuery, conn, tx);
            loanCmd.Parameters.AddWithValue("@bookId", bookId);
            loanCmd.Parameters.AddWithValue("@userId", userId);
            loanCmd.Parameters.AddWithValue("@borrowDate", DateTime.Now);
            loanCmd.Parameters.AddWithValue("@dueDate", DateTime.Now.AddDays(durationDays));
            loanCmd.ExecuteNonQuery();

            var updateQuery = "UPDATE books SET is_available = FALSE WHERE id = @bookId";
            using var updateCmd = new MySqlCommand(updateQuery, conn, tx);
            updateCmd.Parameters.AddWithValue("@bookId", bookId);
            updateCmd.ExecuteNonQuery();

            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            return false;
        }
    }

    public bool ReturnBook(int loanId)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            var getQuery = "SELECT book_id FROM loans WHERE id = @loanId AND return_date IS NULL";
            using var getCmd = new MySqlCommand(getQuery, conn, tx);
            getCmd.Parameters.AddWithValue("@loanId", loanId);
            var bookIdObj = getCmd.ExecuteScalar();

            if (bookIdObj == null)
                return false;

            int bookId = Convert.ToInt32(bookIdObj);

            var returnQuery = "UPDATE loans SET return_date = @returnDate WHERE id = @loanId";
            using var returnCmd = new MySqlCommand(returnQuery, conn, tx);
            returnCmd.Parameters.AddWithValue("@returnDate", DateTime.Now);
            returnCmd.Parameters.AddWithValue("@loanId", loanId);
            returnCmd.ExecuteNonQuery();

            var updateQuery = "UPDATE books SET is_available = TRUE WHERE id = @bookId";
            using var updateCmd = new MySqlCommand(updateQuery, conn, tx);
            updateCmd.Parameters.AddWithValue("@bookId", bookId);
            updateCmd.ExecuteNonQuery();

            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            return false;
        }
    }

    public List<Loan> GetActiveLoans()
    {
        var list = new List<Loan>();
        using var conn = _db.CreateConnection();
        conn.Open();
        var query = "SELECT l.*, b.title AS book_title, u.username FROM loans l " +
                    "JOIN books b ON l.book_id = b.id " +
                    "JOIN users u ON l.user_id = u.id " +
                    "WHERE l.return_date IS NULL ORDER BY l.due_date";
        using var cmd = new MySqlCommand(query, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(ReadLoan(reader));
        }
        return list;
    }

    public List<Loan> GetAllLoans()
    {
        var list = new List<Loan>();
        using var conn = _db.CreateConnection();
        conn.Open();
        var query = "SELECT l.*, b.title AS book_title, u.username FROM loans l " +
                    "JOIN books b ON l.book_id = b.id " +
                    "JOIN users u ON l.user_id = u.id " +
                    "ORDER BY l.borrow_date DESC";
        using var cmd = new MySqlCommand(query, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(ReadLoan(reader));
        }
        return list;
    }

    private static Loan ReadLoan(MySqlDataReader reader)
    {
        return new Loan
        {
            Id = reader.GetInt32("id"),
            BookId = reader.GetInt32("book_id"),
            UserId = reader.GetInt32("user_id"),
            BorrowDate = reader.GetDateTime("borrow_date"),
            DueDate = reader.GetDateTime("due_date"),
            ReturnDate = reader.IsDBNull(reader.GetOrdinal("return_date")) ? null : reader.GetDateTime("return_date"),
            BookTitle = reader.GetString("book_title"),
            Username = reader.GetString("username")
        };
    }
}
