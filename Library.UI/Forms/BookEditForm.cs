using Library.Core.Models;
using Library.UI.Helpers;

namespace Library.UI.Forms;

public class BookEditForm : Form
{
    private TextBox _txtTitle = null!;
    private TextBox _txtAuthor = null!;
    private TextBox _txtGenre = null!;
    private TextBox _txtIsbn = null!;
    private TextBox _txtLocation = null!;

    public string BookTitle => _txtTitle.Text.Trim();
    public string BookAuthor => _txtAuthor.Text.Trim();
    public string BookGenre => _txtGenre.Text.Trim();
    public string BookIsbn => _txtIsbn.Text.Trim();
    public string BookLocation => _txtLocation.Text.Trim();

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
            _txtLocation.Text = _existingBook.Location;
        }
    }

    private void InitializeUI()
    {
        this.Text = _existingBook == null ? "Ajouter un livre" : "Modifier le livre";
        this.Size = new Size(420, 420);
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

        var (lblTitle, txtTitle) = UIHelper.CreateField("Titre *", 56);
        _txtTitle = txtTitle;

        var (lblAuthor, txtAuthor) = UIHelper.CreateField("Auteur *", 110);
        _txtAuthor = txtAuthor;

        var (lblGenre, txtGenre) = UIHelper.CreateField("Genre", 164, fieldWidth: 140);
        _txtGenre = txtGenre;

        var (lblIsbn, txtIsbn) = UIHelper.CreateField("ISBN", 164, labelX: 180, fieldX: 180, fieldWidth: 140);
        _txtIsbn = txtIsbn;

        var (lblLocation, txtLocation) = UIHelper.CreateField("Emplacement *", 218);
        _txtLocation = txtLocation;

        var btnSave = new Button
        {
            Text = _existingBook == null ? "Ajouter" : "Enregistrer",
            Location = new Point(20, 290),
            Width = 140,
            Height = 40
        };
        UIHelper.StyleSuccessButton(btnSave);
        btnSave.Click += OnSaveClick;

        var btnCancel = new Button
        {
            Text = "Annuler",
            Location = new Point(180, 290),
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
            lblLocation, _txtLocation,
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
        if (string.IsNullOrWhiteSpace(_txtLocation.Text))
        {
            ShowError("L'emplacement est obligatoire.");
            _txtLocation.Focus();
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
