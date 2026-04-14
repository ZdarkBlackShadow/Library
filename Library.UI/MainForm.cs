using Library.Core.Services;
using Library.Core.Models;
using Library.UI.Helpers;
using Library.UI.Forms;

namespace Library.UI;

public partial class MainForm : Form
{
    private readonly BookService _bookService = new();
    private readonly LoanService _loanService = new();

    // ── Sidebar ──
    private Panel _sidebar = null!;
    private Button _btnNavBooks = null!;
    private Button _btnNavLoans = null!;

    // ── Content panels ──
    private Panel _contentArea = null!;
    private Panel _booksPanel = null!;
    private Panel _loansPanel = null!;

    // ── Books view ──
    private DataGridView _gridBooks = null!;
    private TextBox _txtSearchTitle = null!;
    private TextBox _txtSearchAuthor = null!;
    private TextBox _txtSearchGenre = null!;
    private TextBox _txtSearchIsbn = null!;
    private Label _lblBookCount = null!;

    // ── Loans view ──
    private DataGridView _gridLoans = null!;
    private Label _lblLoanCount = null!;

    public MainForm()
    {
        this.Text = "Rat de Bibliotheque - Inventaire";
        this.Size = new Size(1100, 680);
        this.MinimumSize = new Size(900, 550);
        UIHelper.StyleForm(this);

        BuildLayout();
        ShowBooksPanel();
        RefreshBooks();
    }

    // ═══════════════════════════════════════════════════════════════
    //  LAYOUT CONSTRUCTION
    // ═══════════════════════════════════════════════════════════════

    private void BuildLayout()
    {
        // Suspend layout during construction to prevent flicker under Wine
        this.SuspendLayout();
        BuildSidebar();
        BuildContentArea();
        BuildBooksPanel();
        BuildLoansPanel();
        this.ResumeLayout(true);
    }

    // ── Sidebar ────────────────────────────────────────────────────
    private void BuildSidebar()
    {
        _sidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 220,
            BackColor = UIHelper.SidebarBg
        };

        // App title
        var lblAppTitle = new Label
        {
            Text = "Rat de\nBibliotheque",
            Dock = DockStyle.Top,
            Height = 80,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(0, 10, 0, 0)
        };

        var separator = new Panel
        {
            Dock = DockStyle.Top,
            Height = 1,
            BackColor = UIHelper.SidebarHover
        };

        // Navigation buttons
        _btnNavLoans = new Button { Text = "    Emprunts" };
        UIHelper.StyleSidebarButton(_btnNavLoans);
        _btnNavLoans.Click += (_, _) => ShowLoansPanel();

        _btnNavBooks = new Button { Text = "    Livres" };
        UIHelper.StyleSidebarButton(_btnNavBooks, isActive: true);
        _btnNavBooks.Click += (_, _) => ShowBooksPanel();

        // Order matters: Top-docked items stack from top
        _sidebar.Controls.Add(_btnNavLoans);
        _sidebar.Controls.Add(_btnNavBooks);
        _sidebar.Controls.Add(separator);
        _sidebar.Controls.Add(lblAppTitle);

        this.Controls.Add(_sidebar);
    }

    // ── Content Area ───────────────────────────────────────────────
    private void BuildContentArea()
    {
        _contentArea = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UIHelper.ContentBg,
            Padding = new Padding(20)
        };
        UIHelper.EnableDoubleBuffered(_contentArea);
        this.Controls.Add(_contentArea);
    }

    // ── Books Panel ────────────────────────────────────────────────
    private void BuildBooksPanel()
    {
        _booksPanel = new Panel { Dock = DockStyle.Fill, BackColor = UIHelper.ContentBg };

        // ── Header ──
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = UIHelper.ContentBg
        };

        var lblTitle = new Label
        {
            Text = "Gestion des Livres",
            Location = new Point(0, 8),
            AutoSize = true
        };
        UIHelper.StyleTitleLabel(lblTitle);

        _lblBookCount = new Label
        {
            Text = "",
            Location = new Point(250, 16),
            AutoSize = true
        };
        UIHelper.StyleSubtitleLabel(_lblBookCount);

        header.Controls.AddRange(new Control[] { lblTitle, _lblBookCount });

        // ── Search card ──
        var searchCard = new Panel
        {
            Dock = DockStyle.Top,
            Height = 80,
            BackColor = UIHelper.CardBg,
            Padding = new Padding(16, 12, 16, 12)
        };

        var lblSearch = new Label
        {
            Text = "Rechercher",
            Location = new Point(16, 8),
            AutoSize = true
        };
        UIHelper.StyleLabel(lblSearch, UIHelper.FontBold, UIHelper.TextSecondary);

        _txtSearchTitle = new TextBox
        {
            PlaceholderText = "Titre...",
            Location = new Point(16, 32),
            Width = 180
        };
        UIHelper.StyleTextBox(_txtSearchTitle);
        _txtSearchTitle.TextChanged += (_, _) => RefreshBooks();

        _txtSearchAuthor = new TextBox
        {
            PlaceholderText = "Auteur...",
            Location = new Point(210, 32),
            Width = 160
        };
        UIHelper.StyleTextBox(_txtSearchAuthor);
        _txtSearchAuthor.TextChanged += (_, _) => RefreshBooks();

        _txtSearchGenre = new TextBox
        {
            PlaceholderText = "Genre...",
            Location = new Point(384, 32),
            Width = 130
        };
        UIHelper.StyleTextBox(_txtSearchGenre);
        _txtSearchGenre.TextChanged += (_, _) => RefreshBooks();

        _txtSearchIsbn = new TextBox
        {
            PlaceholderText = "ISBN...",
            Location = new Point(528, 32),
            Width = 140
        };
        UIHelper.StyleTextBox(_txtSearchIsbn);
        _txtSearchIsbn.TextChanged += (_, _) => RefreshBooks();

        var btnClear = new Button
        {
            Text = "Effacer",
            Location = new Point(682, 30),
            Width = 80,
            Height = 32
        };
        UIHelper.StyleSecondaryButton(btnClear);
        btnClear.Click += OnClearSearch;

        searchCard.Controls.AddRange(new Control[]
        {
            lblSearch, _txtSearchTitle, _txtSearchAuthor,
            _txtSearchGenre, _txtSearchIsbn, btnClear
        });

        // ── Action bar ──
        var actionBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 52,
            BackColor = UIHelper.ContentBg,
            Padding = new Padding(0, 8, 0, 8)
        };

        var btnAdd = new Button { Text = "+ Ajouter", Location = new Point(0, 8), Width = 120 };
        UIHelper.StyleSuccessButton(btnAdd);
        btnAdd.Click += OnAddBook;

        var btnEdit = new Button { Text = "Modifier", Location = new Point(130, 8), Width = 110 };
        UIHelper.StylePrimaryButton(btnEdit);
        btnEdit.Click += OnEditBook;

        var btnDelete = new Button { Text = "Supprimer", Location = new Point(250, 8), Width = 110 };
        UIHelper.StyleDangerButton(btnDelete);
        btnDelete.Click += OnDeleteBook;

        var btnBorrow = new Button { Text = "Emprunter", Location = new Point(380, 8), Width = 120 };
        UIHelper.StyleWarningButton(btnBorrow);
        btnBorrow.Click += OnBorrowBook;

        actionBar.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnBorrow });

        // ── DataGridView ──
        _gridBooks = new DataGridView { Dock = DockStyle.Fill };
        UIHelper.StyleDataGridView(_gridBooks);
        _gridBooks.CellFormatting += OnBooksCellFormatting;

        // ── Assembly order (reverse for Dock) ──
        _booksPanel.Controls.Add(_gridBooks);
        _booksPanel.Controls.Add(actionBar);
        _booksPanel.Controls.Add(searchCard);
        _booksPanel.Controls.Add(header);
    }

    // ── Loans Panel ────────────────────────────────────────────────
    private void BuildLoansPanel()
    {
        _loansPanel = new Panel { Dock = DockStyle.Fill, BackColor = UIHelper.ContentBg };

        // ── Header ──
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = UIHelper.ContentBg
        };

        var lblTitle = new Label
        {
            Text = "Gestion des Emprunts",
            Location = new Point(0, 8),
            AutoSize = true
        };
        UIHelper.StyleTitleLabel(lblTitle);

        _lblLoanCount = new Label
        {
            Text = "",
            Location = new Point(280, 16),
            AutoSize = true
        };
        UIHelper.StyleSubtitleLabel(_lblLoanCount);

        header.Controls.AddRange(new Control[] { lblTitle, _lblLoanCount });

        // ── Action bar ──
        var actionBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 52,
            BackColor = UIHelper.ContentBg,
            Padding = new Padding(0, 8, 0, 8)
        };

        var btnReturn = new Button { Text = "Retourner le livre", Location = new Point(0, 8), Width = 160 };
        UIHelper.StyleSuccessButton(btnReturn);
        btnReturn.Click += OnReturnBook;

        var btnRefresh = new Button { Text = "Actualiser", Location = new Point(170, 8), Width = 110 };
        UIHelper.StyleSecondaryButton(btnRefresh);
        btnRefresh.Click += (_, _) => RefreshLoans();

        actionBar.Controls.AddRange(new Control[] { btnReturn, btnRefresh });

        // ── DataGridView ──
        _gridLoans = new DataGridView { Dock = DockStyle.Fill };
        UIHelper.StyleDataGridView(_gridLoans);
        _gridLoans.CellFormatting += OnLoansCellFormatting;

        // ── Assembly ──
        _loansPanel.Controls.Add(_gridLoans);
        _loansPanel.Controls.Add(actionBar);
        _loansPanel.Controls.Add(header);
    }

    // ═══════════════════════════════════════════════════════════════
    //  NAVIGATION
    // ═══════════════════════════════════════════════════════════════

    private void ShowBooksPanel()
    {
        _contentArea.SuspendLayout();
        _contentArea.Controls.Clear();
        _contentArea.Controls.Add(_booksPanel);
        _contentArea.ResumeLayout(true);
        UIHelper.StyleSidebarButton(_btnNavBooks, isActive: true);
        UIHelper.StyleSidebarButton(_btnNavLoans, isActive: false);
        RefreshBooks();
    }

    private void ShowLoansPanel()
    {
        _contentArea.SuspendLayout();
        _contentArea.Controls.Clear();
        _contentArea.Controls.Add(_loansPanel);
        _contentArea.ResumeLayout(true);
        UIHelper.StyleSidebarButton(_btnNavBooks, isActive: false);
        UIHelper.StyleSidebarButton(_btnNavLoans, isActive: true);
        RefreshLoans();
    }

    // ═══════════════════════════════════════════════════════════════
    //  BOOKS CRUD
    // ═══════════════════════════════════════════════════════════════

    private void RefreshBooks()
    {
        try
        {
            string title = _txtSearchTitle?.Text?.Trim() ?? "";
            string author = _txtSearchAuthor?.Text?.Trim() ?? "";
            string genre = _txtSearchGenre?.Text?.Trim() ?? "";
            string isbn = _txtSearchIsbn?.Text?.Trim() ?? "";

            bool hasFilter = !string.IsNullOrEmpty(title) || !string.IsNullOrEmpty(author) ||
                             !string.IsNullOrEmpty(genre) || !string.IsNullOrEmpty(isbn);

            var books = hasFilter
                ? _bookService.SearchBooks(title, author, genre, isbn)
                : _bookService.GetAllBooks();

            _gridBooks.DataSource = null;
            _gridBooks.DataSource = books;

            if (_gridBooks.Columns.Count > 0)
            {
                SetColumn(_gridBooks, "Id", visible: false);
                SetColumn(_gridBooks, "Title", "Titre", 30);
                SetColumn(_gridBooks, "Author", "Auteur", 20);
                SetColumn(_gridBooks, "Genre", "Genre", 14);
                SetColumn(_gridBooks, "ISBN", "ISBN", 16);
                SetColumn(_gridBooks, "Location", "Emplacement", 20);
                SetColumn(_gridBooks, "IsAvailable", "Dispo.", 8);
            }

            _lblBookCount.Text = $"{books.Count} livre{(books.Count > 1 ? "s" : "")}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnAddBook(object? sender, EventArgs e)
    {
        using var form = new BookEditForm();
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                _bookService.AddBook(form.BookTitle, form.BookAuthor, form.BookGenre, form.BookIsbn, form.BookLocation);
                RefreshBooks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ajout : {ex.Message}", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void OnEditBook(object? sender, EventArgs e)
    {
        var book = GetSelectedBook();
        if (book == null) return;

        using var form = new BookEditForm(book);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                _bookService.UpdateBook(book.Id, form.BookTitle, form.BookAuthor,
                    form.BookGenre, form.BookIsbn, form.BookLocation);
                RefreshBooks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la modification : {ex.Message}", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void OnDeleteBook(object? sender, EventArgs e)
    {
        var book = GetSelectedBook();
        if (book == null) return;

        var result = MessageBox.Show(
            $"Voulez-vous vraiment supprimer \"{book.Title}\" ?",
            "Confirmer la suppression",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result == DialogResult.Yes)
        {
            try
            {
                _bookService.DeleteBook(book.Id);
                RefreshBooks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la suppression : {ex.Message}", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void OnBorrowBook(object? sender, EventArgs e)
    {
        var book = GetSelectedBook();
        if (book == null) return;

        if (!book.IsAvailable)
        {
            MessageBox.Show("Ce livre est deja emprunte.", "Non disponible",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // For simplicity, use user ID 1 (in a real app, pass the logged-in user ID)
        try
        {
            if (_loanService.BorrowBook(book.Id, 1))
            {
                MessageBox.Show($"\"{book.Title}\" a ete emprunte pour 14 jours.",
                    "Emprunt enregistre", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshBooks();
            }
            else
            {
                MessageBox.Show("Impossible d'emprunter ce livre.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnClearSearch(object? sender, EventArgs e)
    {
        _txtSearchTitle.Clear();
        _txtSearchAuthor.Clear();
        _txtSearchGenre.Clear();
        _txtSearchIsbn.Clear();
        RefreshBooks();
    }

    private Book? GetSelectedBook()
    {
        if (_gridBooks.CurrentRow == null || _gridBooks.CurrentRow.DataBoundItem is not Book book)
        {
            MessageBox.Show("Veuillez selectionner un livre.", "Aucune selection",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null;
        }
        return book;
    }

    // ═══════════════════════════════════════════════════════════════
    //  LOANS
    // ═══════════════════════════════════════════════════════════════

    private void RefreshLoans()
    {
        try
        {
            var loans = _loanService.GetAllLoans();
            _gridLoans.DataSource = null;
            _gridLoans.DataSource = loans;

            if (_gridLoans.Columns.Count > 0)
            {
                SetColumn(_gridLoans, "Id", visible: false);
                SetColumn(_gridLoans, "BookId", visible: false);
                SetColumn(_gridLoans, "UserId", visible: false);
                SetColumn(_gridLoans, "IsActive", visible: false);
                SetColumn(_gridLoans, "BookTitle", "Livre", 30);
                SetColumn(_gridLoans, "Username", "Emprunteur", 18);
                SetColumn(_gridLoans, "BorrowDate", "Date d'emprunt", 16, "dd/MM/yyyy");
                SetColumn(_gridLoans, "DueDate", "Date de retour prevue", 16, "dd/MM/yyyy");
                SetColumn(_gridLoans, "ReturnDate", "Rendu le", 14, "dd/MM/yyyy");
                SetColumn(_gridLoans, "IsOverdue", "En retard", 10);
            }

            int activeCount = loans.Count(l => l.IsActive);
            _lblLoanCount.Text = $"{activeCount} emprunt{(activeCount > 1 ? "s" : "")} actif{(activeCount > 1 ? "s" : "")}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnReturnBook(object? sender, EventArgs e)
    {
        if (_gridLoans.CurrentRow == null || _gridLoans.CurrentRow.DataBoundItem is not Loan loan)
        {
            MessageBox.Show("Veuillez selectionner un emprunt.", "Aucune selection",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!loan.IsActive)
        {
            MessageBox.Show("Ce livre a deja ete retourne.", "Deja retourne",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            if (_loanService.ReturnBook(loan.Id))
            {
                MessageBox.Show($"\"{loan.BookTitle}\" a ete retourne.", "Retour enregistre",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshLoans();
            }
            else
            {
                MessageBox.Show("Erreur lors du retour.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  CELL FORMATTING
    // ═══════════════════════════════════════════════════════════════

    private void OnBooksCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.CellStyle == null) return;
        if (e.ColumnIndex < 0 || e.ColumnIndex >= _gridBooks.Columns.Count) return;

        if (_gridBooks.Columns[e.ColumnIndex].Name == "IsAvailable" && e.Value is bool available)
        {
            e.Value = available ? "Oui" : "Non";
            e.FormattingApplied = true;
            e.CellStyle.ForeColor = available ? UIHelper.Success : UIHelper.Danger;
            if (!available) e.CellStyle.Font = UIHelper.FontBold;
        }
    }

    private void OnLoansCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.CellStyle == null) return;
        if (e.RowIndex < 0 || e.RowIndex >= _gridLoans.Rows.Count) return;
        if (e.ColumnIndex < 0 || e.ColumnIndex >= _gridLoans.Columns.Count) return;

        if (_gridLoans.Rows[e.RowIndex].DataBoundItem is Loan loan && loan.IsOverdue)
        {
            e.CellStyle.BackColor = UIHelper.OverdueColor;
        }

        if (_gridLoans.Columns[e.ColumnIndex].Name == "IsOverdue" && e.Value is bool overdue)
        {
            e.Value = overdue ? "Oui" : "Non";
            e.FormattingApplied = true;
            if (overdue)
            {
                e.CellStyle.ForeColor = UIHelper.Danger;
                e.CellStyle.Font = UIHelper.FontBold;
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  HELPERS
    // ═══════════════════════════════════════════════════════════════

    private static void SetColumn(DataGridView grid, string name, string? header = null,
        float fillWeight = 0, string? format = null, bool visible = true)
    {
        var col = grid.Columns[name];
        if (col == null) return;

        col.Visible = visible;
        if (!visible) return;

        if (header != null) col.HeaderText = header;
        if (fillWeight > 0) col.FillWeight = fillWeight;
        if (format != null) col.DefaultCellStyle.Format = format;
    }
}
