using Library.Core.Data;
using Library.Core.Models;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;

namespace Library.Core.Services;

public class BookService
{
    private readonly DatabaseContext _db = new();

    public bool AddBook(string title, string author, string location)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        var query = "INSERT INTO books (title, author, location_description) VALUES (@title, @author, @loc)";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@title", title);
        cmd.Parameters.AddWithValue("@author", author);
        cmd.Parameters.AddWithValue("@loc", location);
        
        return cmd.ExecuteNonQuery() > 0;
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
            list.Add(new Book {
                Id = reader.GetInt32("id"),
                Title = reader.GetString("title"),
                Author = reader.GetString("author"),
                Location = reader.GetString("location_description")
            });
        }
        return list;
    }

    public List<Book> SearchBooks(string filter)
    {
        var list = new List<Book>();
        using var conn = _db.CreateConnection();
        conn.Open();
        var query = "SELECT * FROM books WHERE title LIKE @filter OR author LIKE @filter";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@filter", $"%{filter}%");

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(new Book
            {
                Id = reader.GetInt32("id"),
                Title = reader.GetString("title"),
                Author = reader.GetString("author"),
                Location = reader.GetString("location_description")

            });
        }
        return list;
    }
}
