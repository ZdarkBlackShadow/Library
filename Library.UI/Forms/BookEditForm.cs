using Library.Core.Models;
using Library.UI.Helpers;

namespace Library.UI.Forms;

public class BookEditForm : Form
{
    private TextBox _txtTitle = null!;
    private TextBox _txtAuthor = null!;
    private TextBox _txtGenre = null!;
    private TextBox _txtIsbn = null!;
    private TextBox _txtPublicationYear = null!;
    private TextBox _txtRayon = null!;
    private TextBox _txtEtagere = null!;

    public string BookTitle => _txtTitle.Text.Trim();
    public string BookAuthor => _txtAuthor.Text.Trim();
    public string BookGenre => _txtGenre.Text.Trim();
    public string BookIsbn => _txtIsbn.Text.Trim();
    public int? BookPublicationYear
    {
        get
        {
            string raw = _txtPublicationYear.Text.Trim();
            return int.TryParse(raw, out int y) && y >= 1000 && y <= 2100 ? y : null;
        }
    }
    public string BookRayon => _txtRayon.Text.Trim();
    public string BookEtagere => _txtEtagere.Text.Trim();

    private readonly Book? _existingBook;

    public BookEditForm(Book? book = null)
    {
        _existingBook = book;
        InitializeUI();

        if (_existingBook != null)
        {
            _txtTitle.Text = _existingBook.Title;
            _txtAuthor.Text = _existingBook.Author;
            _txtGenre.Text = _existingBook.Genre ?? "";
            _txtIsbn.Text = _existingBook.ISBN ?? "";
            _txtPublicationYear.Text = _existingBook.PublicationYear?.ToString() ?? "";
            _txtRayon.Text = _existingBook.Rayon;
            _txtEtagere.Text = _existingBook.Etagere;
        }
    }

    private void InitializeUI()
    {
        this.Text = _existingBook == null ? "Ajouter un livre" : "Modifier le livre";
        this.Size = new Size(440, 510);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        UIHelper.StyleForm(this);
        this.BackColor = UIHelper.CardBg;
        this.StartPosition = FormStartPosition.CenterParent;

        var title = new Label
        {
            Text = _existingBook == null ? "Nouveau livre" : "Modifier le livre",
            Location = new Point(20, 16),
            AutoSize = true
        };
        UIHelper.StyleTitleLabel(title);

        // Row 1 — Titre
        var (lblTitle, txtTitle) = UIHelper.CreateField("Titre *", 56, fieldWidth: 380);
        _txtTitle = txtTitle;

        // Row 2 — Auteur
        var (lblAuthor, txtAuthor) = UIHelper.CreateField("Auteur *", 110, fieldWidth: 380);
        _txtAuthor = txtAuthor;

        // Row 3 — Genre (half) + ISBN (half)
        var (lblGenre, txtGenre) = UIHelper.CreateField("Genre", 164, fieldWidth: 140);
        _txtGenre = txtGenre;

        var (lblIsbn, txtIsbn) = UIHelper.CreateField("ISBN", 164, labelX: 180, fieldX: 180, fieldWidth: 140);
        _txtIsbn = txtIsbn;

        // Row 4 — Année de publication (short)
        var (lblYear, txtYear) = UIHelper.CreateField("Année de publication", 218, fieldWidth: 90);
        _txtPublicationYear = txtYear;
        _txtPublicationYear.PlaceholderText = "Ex : 1984";
        _txtPublicationYear.MaxLength = 4;

        // Row 5 — Rayon
        var (lblRayon, txtRayon) = UIHelper.CreateField("Rayon *", 272, fieldWidth: 380);
        _txtRayon = txtRayon;
        _txtRayon.PlaceholderText = "Ex : Salon, Science-Fiction...";

        // Row 6 — Étagère
        var (lblEtagere, txtEtagere) = UIHelper.CreateField("Étagère *", 326, fieldWidth: 380);
        _txtEtagere = txtEtagere;
        _txtEtagere.PlaceholderText = "Ex : Étagère A1, Meuble Haut...";

        // Buttons
        var btnSave = new Button
        {
            Text = _existingBook == null ? "Ajouter" : "Enregistrer",
            Location = new Point(20, 390),
            Width = 160,
            Height = 40
        };
        UIHelper.StyleSuccessButton(btnSave);
        btnSave.Click += OnSaveClick;

        var btnCancel = new Button
        {
            Text = "Annuler",
            Location = new Point(200, 390),
            Width = 140,
            Height = 40
        };
        UIHelper.StyleSecondaryButton(btnCancel);
        btnCancel.Click += (_, _) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

        this.Controls.AddRange(new Control[]
        {
            title,
            lblTitle, _txtTitle,
            lblAuthor, _txtAuthor,
            lblGenre, _txtGenre,
            lblIsbn, _txtIsbn,
            lblYear, _txtPublicationYear,
            lblRayon, _txtRayon,
            lblEtagere, _txtEtagere,
            btnSave, btnCancel
        });

        this.AcceptButton = btnSave;
    }

    private void OnSaveClick(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtTitle.Text))
        {
            ShowError("Le titre est obligatoire.");
            _txtTitle.Focus();
            return;
        }
        if (string.IsNullOrWhiteSpace(_txtAuthor.Text))
        {
            ShowError("L'auteur est obligatoire.");
            _txtAuthor.Focus();
            return;
        }

        string rawYear = _txtPublicationYear.Text.Trim();
        if (!string.IsNullOrEmpty(rawYear))
        {
            if (!int.TryParse(rawYear, out int y) || y < 1000 || y > 2100)
            {
                ShowError("L'année de publication doit être un nombre entre 1000 et 2100.");
                _txtPublicationYear.Focus();
                return;
            }
        }

        if (string.IsNullOrWhiteSpace(_txtRayon.Text))
        {
            ShowError("Le rayon est obligatoire.");
            _txtRayon.Focus();
            return;
        }
        if (string.IsNullOrWhiteSpace(_txtEtagere.Text))
        {
            ShowError("L'étagère est obligatoire.");
            _txtEtagere.Focus();
            return;
        }

        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    private static void ShowError(string message)
    {
        MessageBox.Show(message, "Champ requis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
