using Library.Core.Data;
using Library.Core.Models;
using MySql.Data.MySqlClient;

namespace Library.Core.Services;

public class BookService
{
    private readonly DatabaseContext _db = new();

    public bool AddBook(string title, string author, string genre, string isbn, string location)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var query = "INSERT INTO books (title, author, genre, isbn, location_description) VALUES (@title, @author, @genre, @isbn, @loc)";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@title", title);
        cmd.Parameters.AddWithValue("@author", author);
        cmd.Parameters.AddWithValue("@genre", string.IsNullOrWhiteSpace(genre) ? DBNull.Value : genre);
        cmd.Parameters.AddWithValue("@isbn", string.IsNullOrWhiteSpace(isbn) ? DBNull.Value : isbn);
        cmd.Parameters.AddWithValue("@loc", location);

        return cmd.ExecuteNonQuery() > 0;
    }

    public bool UpdateBook(int id, string title, string author, string genre, string isbn, string location)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var query = "UPDATE books SET title = @title, author = @author, genre = @genre, isbn = @isbn, location_description = @loc WHERE id = @id";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@title", title);
        cmd.Parameters.AddWithValue("@author", author);
        cmd.Parameters.AddWithValue("@genre", string.IsNullOrWhiteSpace(genre) ? DBNull.Value : genre);
        cmd.Parameters.AddWithValue("@isbn", string.IsNullOrWhiteSpace(isbn) ? DBNull.Value : isbn);
        cmd.Parameters.AddWithValue("@loc", location);

        return cmd.ExecuteNonQuery() > 0;
    }

    public bool DeleteBook(int id)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var query = "DELETE FROM books WHERE id = @id";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@id", id);

        return cmd.ExecuteNonQuery() > 0;
    }

    public Book? GetBookById(int id)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var query = "SELECT * FROM books WHERE id = @id";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
            return ReadBook(reader);

        return null;
    }

    public List<Book> GetAllBooks()
    {
        var list = new List<Book>();
        using var conn = _db.CreateConnection();
        conn.Open();
        var query = "SELECT * FROM books";
        using var cmd = new MySqlCommand(query, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(ReadBook(reader));
        }
        return list;
    }

    public List<Book> SearchBooks(string? title, string? author, string? genre, string? isbn)
    {
        var list = new List<Book>();
        var conditions = new List<string>();
        using var conn = _db.CreateConnection();
        conn.Open();

        var query = "SELECT * FROM books";
        using var cmd = new MySqlCommand();
        cmd.Connection = conn;

        if (!string.IsNullOrWhiteSpace(title))
        {
            conditions.Add("title LIKE @title");
            cmd.Parameters.AddWithValue("@title", $"%{title}%");
        }
        if (!string.IsNullOrWhiteSpace(author))
        {
            conditions.Add("author LIKE @author");
            cmd.Parameters.AddWithValue("@author", $"%{author}%");
        }
        if (!string.IsNullOrWhiteSpace(genre))
        {
            conditions.Add("genre LIKE @genre");
            cmd.Parameters.AddWithValue("@genre", $"%{genre}%");
        }
        if (!string.IsNullOrWhiteSpace(isbn))
        {
            conditions.Add("isbn LIKE @isbn");
            cmd.Parameters.AddWithValue("@isbn", $"%{isbn}%");
        }

        if (conditions.Count > 0)
            query += " WHERE " + string.Join(" AND ", conditions);

        cmd.CommandText = query;
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(ReadBook(reader));
        }
        return list;
    }

    private static Book ReadBook(MySqlDataReader reader)
    {
        return new Book
        {
            Id = reader.GetInt32("id"),
            Title = reader.GetString("title"),
            Author = reader.GetString("author"),
            Genre = reader.IsDBNull(reader.GetOrdinal("genre")) ? null : reader.GetString("genre"),
            ISBN = reader.IsDBNull(reader.GetOrdinal("isbn")) ? null : reader.GetString("isbn"),
            Location = reader.GetString("location_description"),
            IsAvailable = reader.GetBoolean("is_available")
        };
    }
}
