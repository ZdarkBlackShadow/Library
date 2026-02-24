using Library.Core.Services;
using Library.Core.Models;

namespace Library.UI;

public partial class MainForm : Form
{
    private readonly BookService _bookService = new();
    private DataGridView _gridBooks = null!;
    private TextBox _txtSearch = null!;

    public MainForm()
    {
        this.Text = "Mon Inventaire de Rat de Bibliothèque";
        this.Size = new Size(800, 500);
        this.StartPosition = FormStartPosition.CenterScreen;

        InitializeComponents();
        RefreshGrid();
    }

    private void InitializeComponents()
    {
        _txtSearch = new TextBox { Location = new Point(20, 20), Width = 300 };
        _txtSearch.PlaceholderText = "Rechercher un titre ou un auteur...";
        _txtSearch.TextChanged += (s, e) => RefreshGrid(_txtSearch.Text);

        _gridBooks = new DataGridView {
            Location = new Point(20, 60),
            Size = new Size(740, 350),
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };
        _gridBooks.DefaultCellStyle.Font = new Font("Arial", 9);
        this.Controls.Add(_txtSearch);
        this.Controls.Add(_gridBooks);
    }

    private void RefreshGrid(string filter = "")
    {
        var books = string.IsNullOrWhiteSpace(filter) 
            ? _bookService.GetAllBooks() 
            : _bookService.SearchBooks(filter);
            
        _gridBooks.DataSource = null;
        _gridBooks.DataSource = books;

        if (_gridBooks.Columns.Count > 0)
        {
            if (_gridBooks.Columns.Contains("ISBN")) 
            {
                _gridBooks.Columns["ISBN"].HeaderText = "ISBN";
                _gridBooks.Columns["ISBN"].Visible = false;
            }

            _gridBooks.Columns["Id"].Visible = false;
            _gridBooks.Columns["Title"].HeaderText = "Titre du livre";
            _gridBooks.Columns["Author"].HeaderText = "Auteur";
            _gridBooks.Columns["Location"].HeaderText = "Emplacement";
        }
    }
}
