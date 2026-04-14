using System.Data.Common;
using Library.Core.Data;
using Library.Core.Models;
using MySql.Data.MySqlClient;

namespace Library.Core.Services;

public class BookService
{
    private readonly DatabaseContext _db = new();

    public async Task<bool> AddBook(string title, string author, string? genre, string? isbn,
        int? publicationYear, string rayon, string etagere)
    {
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        const string query =
            "INSERT INTO books (title, author, genre, isbn, publication_year, rayon, etagere) " +
            "VALUES (@title, @author, @genre, @isbn, @year, @rayon, @etagere)";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@title", title);
        cmd.Parameters.AddWithValue("@author", author);
        cmd.Parameters.AddWithValue("@genre", string.IsNullOrWhiteSpace(genre) ? DBNull.Value : (object)genre);
        cmd.Parameters.AddWithValue("@isbn", string.IsNullOrWhiteSpace(isbn) ? DBNull.Value : (object)isbn);
        cmd.Parameters.AddWithValue("@year", publicationYear.HasValue ? (object)publicationYear.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@rayon", rayon);
        cmd.Parameters.AddWithValue("@etagere", etagere);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> UpdateBook(int id, string title, string author, string? genre, string? isbn,
        int? publicationYear, string rayon, string etagere)
    {
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        const string query =
            "UPDATE books SET title=@title, author=@author, genre=@genre, isbn=@isbn, " +
            "publication_year=@year, rayon=@rayon, etagere=@etagere WHERE id=@id";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@title", title);
        cmd.Parameters.AddWithValue("@author", author);
        cmd.Parameters.AddWithValue("@genre", string.IsNullOrWhiteSpace(genre) ? DBNull.Value : (object)genre);
        cmd.Parameters.AddWithValue("@isbn", string.IsNullOrWhiteSpace(isbn) ? DBNull.Value : (object)isbn);
        cmd.Parameters.AddWithValue("@year", publicationYear.HasValue ? (object)publicationYear.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@rayon", rayon);
        cmd.Parameters.AddWithValue("@etagere", etagere);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteBook(int id)
    {
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        const string query = "DELETE FROM books WHERE id = @id";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@id", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<Book?> GetBookById(int id)
    {
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        const string query = "SELECT * FROM books WHERE id = @id";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return ReadBook(reader);
        return null;
    }

    public async Task<List<Book>> GetAllBooks()
    {
        var list = new List<Book>();
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        const string query = "SELECT * FROM books ORDER BY title";
        using var cmd = new MySqlCommand(query, conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add(ReadBook(reader));
        return list;
    }

    public async Task<List<Book>> SearchBooks(string? title, string? author, string? genre, string? isbn)
    {
        var list = new List<Book>();
        var conditions = new List<string>();

        using var conn = _db.CreateConnection();
        await conn.OpenAsync();

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

        string query = "SELECT * FROM books";
        if (conditions.Count > 0)
            query += " WHERE " + string.Join(" AND ", conditions);
        query += " ORDER BY title";

        cmd.CommandText = query;
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add(ReadBook(reader));
        return list;
    }

    private static Book ReadBook(DbDataReader reader)
    {
        int yearOrdinal = reader.GetOrdinal("publication_year");
        return new Book
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            Title = reader.GetString(reader.GetOrdinal("title")),
            Author = reader.GetString(reader.GetOrdinal("author")),
            Genre = reader.IsDBNull(reader.GetOrdinal("genre"))
                ? null : reader.GetString(reader.GetOrdinal("genre")),
            ISBN = reader.IsDBNull(reader.GetOrdinal("isbn"))
                ? null : reader.GetString(reader.GetOrdinal("isbn")),
            PublicationYear = reader.IsDBNull(yearOrdinal)
                ? null : reader.GetInt32(yearOrdinal),
            Rayon = reader.GetString(reader.GetOrdinal("rayon")),
            Etagere = reader.GetString(reader.GetOrdinal("etagere")),
            IsAvailable = reader.GetBoolean(reader.GetOrdinal("is_available"))
        };
    }
}
