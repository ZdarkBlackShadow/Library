using Library.Core.Services;
using Library.UI.Helpers;

namespace Library.UI;

public partial class Form1 : Form
{
    private readonly UserService _userService = new();

    private TextBox _txtUsername = null!;
    private TextBox _txtPassword = null!;
    private Label _lblError = null!;
    private Button _btnLogin = null!;
    private Button _btnRegister = null!;

    public int LoggedInUserId { get; private set; }

    public Form1()
    {
        InitializeComponent();
        BuildUI();
    }

    private void BuildUI()
    {
        this.Text = "Rat de Bibliotheque";
        this.Size = new Size(440, 500);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        UIHelper.StyleForm(this);

        // ── Left accent bar ──
        var accentBar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 6,
            BackColor = UIHelper.Primary
        };

        // ── Center card ──
        var card = new Panel
        {
            Size = new Size(380, 420),
            Location = new Point(30, 20),
            BackColor = UIHelper.CardBg
        };
        UIHelper.StyleCard(card);

        // ── App icon area ──
        var lblIcon = new Label
        {
            Text = "[ ]",
            Font = new Font("Segoe UI", 28, FontStyle.Bold),
            ForeColor = UIHelper.Primary,
            Location = new Point(140, 10),
            AutoSize = true
        };

        var lblAppName = new Label
        {
            Text = "Rat de Bibliotheque",
            Location = new Point(80, 70),
            AutoSize = true
        };
        UIHelper.StyleTitleLabel(lblAppName);
        lblAppName.ForeColor = UIHelper.TextPrimary;

        var lblSubtitle = new Label
        {
            Text = "Connectez-vous pour continuer",
            Location = new Point(98, 100),
            AutoSize = true
        };
        UIHelper.StyleSubtitleLabel(lblSubtitle);

        // ── Fields ──
        var (lblUser, txtUser) = UIHelper.CreateField("Nom d'utilisateur", 140, fieldWidth: 340);
        _txtUsername = txtUser;

        var (lblPass, txtPass) = UIHelper.CreateField("Mot de passe", 200, fieldWidth: 340);
        _txtPassword = txtPass;
        _txtPassword.PasswordChar = '\u25CF';

        // ── Error label ──
        _lblError = new Label
        {
            Text = "",
            Location = new Point(20, 258),
            Size = new Size(340, 20),
            ForeColor = UIHelper.Danger,
            Font = UIHelper.FontSmall,
            Visible = false
        };

        // ── Buttons ──
        _btnLogin = new Button
        {
            Text = "Se connecter",
            Location = new Point(20, 282),
            Width = 340,
            Height = 42
        };
        UIHelper.StylePrimaryButton(_btnLogin);
        _btnLogin.Font = UIHelper.FontBold;
        _btnLogin.Click += OnLoginClick;

        _btnRegister = new Button
        {
            Text = "Creer un compte",
            Location = new Point(20, 334),
            Width = 340,
            Height = 42
        };
        UIHelper.StyleSecondaryButton(_btnRegister);
        _btnRegister.Click += OnRegisterClick;

        card.Controls.AddRange(new Control[]
        {
            lblIcon, lblAppName, lblSubtitle,
            lblUser, _txtUsername,
            lblPass, _txtPassword,
            _lblError,
            _btnLogin, _btnRegister
        });

        this.Controls.Add(card);
        this.Controls.Add(accentBar);

        this.AcceptButton = _btnLogin;
    }

    private void OnLoginClick(object? sender, EventArgs e)
    {
        _lblError.Visible = false;

        if (string.IsNullOrWhiteSpace(_txtUsername.Text) || string.IsNullOrWhiteSpace(_txtPassword.Text))
        {
            ShowError("Veuillez remplir tous les champs.");
            return;
        }

        try
        {
            if (_userService.Login(_txtUsername.Text.Trim(), _txtPassword.Text))
            {
                this.Hide();
                var mainForm = new MainForm();
                mainForm.ShowDialog();
                this.Close();
            }
            else
            {
                ShowError("Identifiants incorrects.");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur de connexion : {ex.Message}");
        }
    }

    private void OnRegisterClick(object? sender, EventArgs e)
    {
        _lblError.Visible = false;

        if (string.IsNullOrWhiteSpace(_txtUsername.Text) || string.IsNullOrWhiteSpace(_txtPassword.Text))
        {
            ShowError("Veuillez remplir tous les champs.");
            return;
        }

        if (_txtPassword.Text.Length < 4)
        {
            ShowError("Le mot de passe doit contenir au moins 4 caracteres.");
            return;
        }

        try
        {
            if (_userService.Register(_txtUsername.Text.Trim(), _txtPassword.Text))
            {
                _lblError.ForeColor = UIHelper.Success;
                _lblError.Text = "Compte cree ! Vous pouvez vous connecter.";
                _lblError.Visible = true;
            }
            else
            {
                ShowError("Ce nom d'utilisateur existe deja.");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur : {ex.Message}");
        }
    }

    private void ShowError(string message)
    {
        _lblError.ForeColor = UIHelper.Danger;
        _lblError.Text = message;
        _lblError.Visible = true;
    }
}
