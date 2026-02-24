using Library.Core.Services;

namespace Library.UI;

public partial class Form1 : Form
{
    private readonly UserService _userService = new();
    
    // Éléments d'interface
    private TextBox txtUsername = null!;
    private TextBox txtPassword = null!;
    private Button btnLogin = null!;
    private Button btnRegister = null!;

    public Form1()
    {
        InitializeCustomComponents();
    }

    private void InitializeCustomComponents()
    {
        this.Text = "Rat de Bibliothèque - Connexion";
        this.Size = new Size(300, 250);
        this.StartPosition = FormStartPosition.CenterScreen;

        var lblUser = new Label { Text = "Utilisateur:", Location = new Point(20, 20), AutoSize = true };
        txtUsername = new TextBox { Location = new Point(20, 40), Width = 240 };

        var lblPass = new Label { Text = "Mot de passe:", Location = new Point(20, 80), AutoSize = true };
        txtPassword = new TextBox { Location = new Point(20, 100), Width = 240, PasswordChar = '●' };

        btnLogin = new Button { Text = "Se connecter", Location = new Point(20, 150), Width = 110 };
        btnRegister = new Button { Text = "S'inscrire", Location = new Point(150, 150), Width = 110 };

        btnLogin.Click += OnLoginClick;
        btnRegister.Click += OnRegisterClick;

        this.Controls.AddRange(new Control[] { lblUser, txtUsername, lblPass, txtPassword, btnLogin, btnRegister });
    }

    private void OnLoginClick(object? sender, EventArgs e)
    {
        if (_userService.Login(txtUsername.Text, txtPassword.Text))
        {
            this.Hide();
            
            var mainForm = new MainForm();
            mainForm.ShowDialog();
            
            this.Close();
        }
        else
        {
            MessageBox.Show("Identifiants incorrects.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnRegisterClick(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text)) return;

        if (_userService.Register(txtUsername.Text, txtPassword.Text))
        {
            MessageBox.Show("Compte créé avec succès ! Vous pouvez vous connecter.", "Succès");
        }
        else
        {
            MessageBox.Show("Erreur lors de l'inscription.", "Erreur");
        }
    }
}