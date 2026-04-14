using Library.Core.Services;
using Library.Core.Models;
using Library.UI.Helpers;
using Library.UI.Forms;

namespace Library.UI;

public partial class MainForm : Form
{
    private readonly BookService _bookService = new();
    private readonly LoanService _loanService = new();
    private readonly int _loggedInUserId;

    // ── Sidebar ──
    private Panel _sidebar = null!;
    private Label _lblSidebarTitle = null!;
    private Button _btnNavBooks = null!;
    private Button _btnNavLoans = null!;
    private Button _btnToggleSidebar = null!;
    private bool _sidebarExpanded = true;
    private const int SidebarExpandedWidth = 220;
    private const int SidebarCollapsedWidth = 60;

    // ── Root layout ──
    private TableLayoutPanel _tableLayout = null!;

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

    public MainForm(int loggedInUserId)
    {
        _loggedInUserId = loggedInUserId;
        this.Text = "Rat de Bibliotheque - Inventaire";
        this.Size = new Size(1200, 680);
        this.MinimumSize = new Size(950, 550);
        UIHelper.StyleForm(this);

        BuildLayout();
        ShowBooksPanel();
        _ = RefreshBooksAsync();
    }

    // ═══════════════════════════════════════════════════════════════
    //  LAYOUT CONSTRUCTION
    // ═══════════════════════════════════════════════════════════════

    private void BuildLayout()
    {
        this.SuspendLayout();

        // ── Root container ────────────────────────────────────────────
        // Using a TableLayoutPanel instead of DockStyle.Left + DockStyle.Fill
        // on sibling panels eliminates the Z-order/docking interaction that
        // Wine's layout pass gets wrong (it processes controls from the highest
        // Controls-collection index toward 0 without a guaranteed two-pass
        // grouping of non-Fill before Fill, so whichever panel ends up at the
        // higher index claims its space first — and when Fill wins that race it
        // takes the full client width, causing the sidebar to overlap instead of
        // push the content aside).
        //
        // A TableLayoutPanel with an Absolute first column and a Percent(100)
        // second column is an explicit spatial contract: no iteration order or
        // Z-order value can override it.
        _tableLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0),
            Padding = new Padding(0),
            CellBorderStyle = TableLayoutPanelCellBorderStyle.None
        };
        UIHelper.EnableDoubleBuffered(_tableLayout);
        _tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SidebarExpandedWidth));
        _tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        _tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        this.Controls.Add(_tableLayout);

        BuildSidebar();      // adds _sidebar to _tableLayout column 0
        BuildContentArea();  // adds _contentArea to _tableLayout column 1
        BuildBooksPanel();
        BuildLoansPanel();

        this.ResumeLayout(true);
    }

    // ── Sidebar ────────────────────────────────────────────────────
    private void BuildSidebar()
    {
        // Dock = Fill so the panel fills its TableLayoutPanel cell completely.
        // The cell width is controlled by _tableLayout.ColumnStyles[0]; no Width
        // property is needed or meaningful here.
        _sidebar = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UIHelper.SidebarBg
        };

        // Stored as a field so OnToggleSidebar can show/hide it
        _lblSidebarTitle = new Label
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

        _btnNavLoans = new Button { Text = "    Emprunts" };
        UIHelper.StyleSidebarButton(_btnNavLoans);
        _btnNavLoans.Click += async (_, _) => { ShowLoansPanel(); await RefreshLoansAsync(); };

        _btnNavBooks = new Button { Text = "    Livres" };
        UIHelper.StyleSidebarButton(_btnNavBooks, isActive: true);
        _btnNavBooks.Click += async (_, _) => { ShowBooksPanel(); await RefreshBooksAsync(); };

        // Toggle button docks to the bottom of the sidebar.
        // StyleSidebarButton sets Dock=Top and TextAlign=MiddleLeft, so we
        // override those two properties right after the style call.
        _btnToggleSidebar = new Button { Text = " «" };
        UIHelper.StyleSidebarButton(_btnToggleSidebar);
        _btnToggleSidebar.Dock = DockStyle.Bottom;
        _btnToggleSidebar.TextAlign = ContentAlignment.MiddleCenter;
        _btnToggleSidebar.Padding = new Padding(0);
        _btnToggleSidebar.Click += OnToggleSidebar;

        // Add order for Top-docked controls: the LAST control added ends up at
        // the visual TOP (it becomes index 0 = processed first by the layout engine).
        // Bottom-docked controls are independent; add the toggle first so it anchors
        // to the very bottom before the Top controls are stacked above it.
        _sidebar.Controls.Add(_btnToggleSidebar); // DockStyle.Bottom → bottom of sidebar
        _sidebar.Controls.Add(_btnNavLoans);       // Top → will be below NavBooks
        _sidebar.Controls.Add(_btnNavBooks);       // Top → will be below separator
        _sidebar.Controls.Add(separator);          // Top → will be below title
        _sidebar.Controls.Add(_lblSidebarTitle);   // Top → sits at the very top

        _tableLayout.Controls.Add(_sidebar, 0, 0); // column 0, row 0
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
        _tableLayout.Controls.Add(_contentArea, 1, 0); // column 1, row 0
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
        _txtSearchTitle.TextChanged += async (_, _) => await RefreshBooksAsync();

        _txtSearchAuthor = new TextBox
        {
            PlaceholderText = "Auteur...",
            Location = new Point(210, 32),
            Width = 160
        };
        UIHelper.StyleTextBox(_txtSearchAuthor);
        _txtSearchAuthor.TextChanged += async (_, _) => await RefreshBooksAsync();

        _txtSearchGenre = new TextBox
        {
            PlaceholderText = "Genre...",
            Location = new Point(384, 32),
            Width = 130
        };
        UIHelper.StyleTextBox(_txtSearchGenre);
        _txtSearchGenre.TextChanged += async (_, _) => await RefreshBooksAsync();

        _txtSearchIsbn = new TextBox
        {
            PlaceholderText = "ISBN...",
            Location = new Point(528, 32),
            Width = 140
        };
        UIHelper.StyleTextBox(_txtSearchIsbn);
        _txtSearchIsbn.TextChanged += async (_, _) => await RefreshBooksAsync();

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

        _booksPanel.Controls.Add(_gridBooks);
        _booksPanel.Controls.Add(actionBar);
        _booksPanel.Controls.Add(searchCard);
        _booksPanel.Controls.Add(header);
    }

    // ── Loans Panel ────────────────────────────────────────────────
    private void BuildLoansPanel()
    {
        _loansPanel = new Panel { Dock = DockStyle.Fill, BackColor = UIHelper.ContentBg };

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
        btnRefresh.Click += async (_, _) => await RefreshLoansAsync();

        actionBar.Controls.AddRange(new Control[] { btnReturn, btnRefresh });

        _gridLoans = new DataGridView { Dock = DockStyle.Fill };
        UIHelper.StyleDataGridView(_gridLoans);
        _gridLoans.CellFormatting += OnLoansCellFormatting;

        _loansPanel.Controls.Add(_gridLoans);
        _loansPanel.Controls.Add(actionBar);
        _loansPanel.Controls.Add(header);
    }

    // ═══════════════════════════════════════════════════════════════
    //  SIDEBAR TOGGLE
    // ═══════════════════════════════════════════════════════════════

    private void OnToggleSidebar(object? sender, EventArgs e)
    {
        _sidebarExpanded = !_sidebarExpanded;

        // Suspend both the form and the table so the column-style change,
        // the visibility toggle, and the text updates all flush in one pass.
        this.SuspendLayout();
        _tableLayout.SuspendLayout();

        if (_sidebarExpanded)
        {
            // Widen the sidebar column — the content column auto-shrinks to fill
            // whatever remains because its ColumnStyle is Percent(100).
            _tableLayout.ColumnStyles[0] = new ColumnStyle(SizeType.Absolute, SidebarExpandedWidth);

            _lblSidebarTitle.Visible = true;

            _btnNavBooks.Text = "    Livres";
            _btnNavBooks.TextAlign = ContentAlignment.MiddleLeft;
            _btnNavBooks.Padding = new Padding(20, 0, 0, 0);

            _btnNavLoans.Text = "    Emprunts";
            _btnNavLoans.TextAlign = ContentAlignment.MiddleLeft;
            _btnNavLoans.Padding = new Padding(20, 0, 0, 0);

            _btnToggleSidebar.Text = " «";
        }
        else
        {
            _tableLayout.ColumnStyles[0] = new ColumnStyle(SizeType.Absolute, SidebarCollapsedWidth);

            _lblSidebarTitle.Visible = false;

            _btnNavBooks.Text = "L";
            _btnNavBooks.TextAlign = ContentAlignment.MiddleCenter;
            _btnNavBooks.Padding = new Padding(0);

            _btnNavLoans.Text = "E";
            _btnNavLoans.TextAlign = ContentAlignment.MiddleCenter;
            _btnNavLoans.Padding = new Padding(0);

            _btnToggleSidebar.Text = " »";
        }

        _tableLayout.ResumeLayout(true);
        this.ResumeLayout(true);
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
        // StyleSidebarButton resets TextAlign/Padding to expanded defaults;
        // restore collapsed state if the sidebar is currently collapsed.
        if (!_sidebarExpanded) RestoreCollapsedButtonStyle();
    }

    private void ShowLoansPanel()
    {
        _contentArea.SuspendLayout();
        _contentArea.Controls.Clear();
        _contentArea.Controls.Add(_loansPanel);
        _contentArea.ResumeLayout(true);
        UIHelper.StyleSidebarButton(_btnNavBooks, isActive: false);
        UIHelper.StyleSidebarButton(_btnNavLoans, isActive: true);
        if (!_sidebarExpanded) RestoreCollapsedButtonStyle();
    }

    private void RestoreCollapsedButtonStyle()
    {
        _btnNavBooks.Text = "L";
        _btnNavBooks.TextAlign = ContentAlignment.MiddleCenter;
        _btnNavBooks.Padding = new Padding(0);
        _btnNavLoans.Text = "E";
        _btnNavLoans.TextAlign = ContentAlignment.MiddleCenter;
        _btnNavLoans.Padding = new Padding(0);
    }

    // ═══════════════════════════════════════════════════════════════
    //  BOOKS CRUD
    // ═══════════════════════════════════════════════════════════════

    private async Task RefreshBooksAsync()
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
                ? await _bookService.SearchBooks(title, author, genre, isbn)
                : await _bookService.GetAllBooks();

            _gridBooks.DataSource = null;
            _gridBooks.DataSource = books;

            if (_gridBooks.Columns.Count > 0)
            {
                // Replace auto-generated CheckBoxColumn with TextBoxColumn so
                // CellFormatting can safely set e.Value to a string ("Oui"/"Non")
                EnsureTextColumn(_gridBooks, "IsAvailable");

                SetColumn(_gridBooks, "Id", visible: false);
                SetColumn(_gridBooks, "Title", "Titre", 20);
                SetColumn(_gridBooks, "Author", "Auteur", 16);
                SetColumn(_gridBooks, "Genre", "Genre", 11);
                SetColumn(_gridBooks, "ISBN", "ISBN", 13);
                SetColumn(_gridBooks, "PublicationYear", "Année", 7);
                SetColumn(_gridBooks, "Rayon", "Rayon", 11);
                SetColumn(_gridBooks, "Etagere", "Étagère", 11);
                SetColumn(_gridBooks, "IsAvailable", "Dispo.", 7);
            }

            _lblBookCount.Text = $"{books.Count} livre{(books.Count > 1 ? "s" : "")}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void OnAddBook(object? sender, EventArgs e)
    {
        using var form = new BookEditForm();
        if (form.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            await _bookService.AddBook(
                form.BookTitle, form.BookAuthor, form.BookGenre, form.BookIsbn,
                form.BookPublicationYear, form.BookRayon, form.BookEtagere);
            await RefreshBooksAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de l'ajout : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void OnEditBook(object? sender, EventArgs e)
    {
        var book = GetSelectedBook();
        if (book == null) return;

        using var form = new BookEditForm(book);
        if (form.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            await _bookService.UpdateBook(
                book.Id, form.BookTitle, form.BookAuthor, form.BookGenre, form.BookIsbn,
                form.BookPublicationYear, form.BookRayon, form.BookEtagere);
            await RefreshBooksAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la modification : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void OnDeleteBook(object? sender, EventArgs e)
    {
        var book = GetSelectedBook();
        if (book == null) return;

        var result = MessageBox.Show(
            $"Voulez-vous vraiment supprimer \"{book.Title}\" ?",
            "Confirmer la suppression",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes) return;

        try
        {
            await _bookService.DeleteBook(book.Id);
            await RefreshBooksAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la suppression : {ex.Message}", "Erreur",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void OnBorrowBook(object? sender, EventArgs e)
    {
        var book = GetSelectedBook();
        if (book == null) return;

        if (!book.IsAvailable)
        {
            MessageBox.Show("Ce livre est deja emprunte.", "Non disponible",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            if (await _loanService.BorrowBook(book.Id, _loggedInUserId))
            {
                MessageBox.Show($"\"{book.Title}\" a ete emprunte pour 14 jours.",
                    "Emprunt enregistre", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await RefreshBooksAsync();
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
        // TextChanged will fire on each Clear() and trigger RefreshBooksAsync automatically
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

    private async Task RefreshLoansAsync()
    {
        try
        {
            var loans = await _loanService.GetAllLoans();
            _gridLoans.DataSource = null;
            _gridLoans.DataSource = loans;

            if (_gridLoans.Columns.Count > 0)
            {
                // Same fix: replace CheckBoxColumns before CellFormatting runs
                EnsureTextColumn(_gridLoans, "IsOverdue");
                EnsureTextColumn(_gridLoans, "IsActive");

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

    private async void OnReturnBook(object? sender, EventArgs e)
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
            if (await _loanService.ReturnBook(loan.Id))
            {
                MessageBox.Show($"\"{loan.BookTitle}\" a ete retourne.", "Retour enregistre",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                await RefreshLoansAsync();
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

    /// <summary>
    /// DataGridView auto-generates a DataGridViewCheckBoxColumn for bool properties.
    /// A CheckBoxColumn only accepts bool/CheckState as formatted values — setting a
    /// string in CellFormatting throws System.FormatException (visible on Wine).
    /// This method replaces the auto-generated column with a plain TextBoxColumn so
    /// CellFormatting can safely write "Oui"/"Non" strings.
    /// </summary>
    private static void EnsureTextColumn(DataGridView grid, string name)
    {
        if (grid.Columns[name] is not DataGridViewCheckBoxColumn checkCol) return;

        int index = checkCol.Index;
        string dataPropertyName = checkCol.DataPropertyName;
        grid.Columns.RemoveAt(index);
        grid.Columns.Insert(index, new DataGridViewTextBoxColumn
        {
            Name = name,
            DataPropertyName = dataPropertyName,
        });
    }
}
