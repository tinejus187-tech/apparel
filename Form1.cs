namespace Justine_Apparel_E_Commerce_System;

public class Form1 : Form
{
    private Panel cardPanel = null!;
    private Label titleLabel = null!;
    private Button adminButton = null!;
    private Button customerButton = null!;
    private Label subtitle = null!;
    private Panel adminPanel = null!;
    private TextBox emailBox = null!;
    private TextBox passwordBox = null!;
    private Button signInButton = null!;
    private Label errorLabel = null!;
    private Button backButton = null!;

    public Form1()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Justine Apparel";
        this.Size = new Size(500, 500);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(240, 242, 245);
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;

        cardPanel = new Panel
        {
            Size = new Size(380, 370),
            Location = new Point((this.ClientSize.Width - 380) / 2, (this.ClientSize.Height - 370) / 2),
            BackColor = Color.White
        };

        titleLabel = new Label
        {
            Text = "Justine Apparel",
            Font = new Font("Segoe UI", 22, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 30),
            Size = new Size(340, 50),
            Location = new Point(20, 35),
            TextAlign = ContentAlignment.MiddleCenter
        };

        subtitle = new Label
        {
            Text = "E-Commerce Management System",
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            ForeColor = Color.FromArgb(130, 130, 130),
            Size = new Size(340, 20),
            Location = new Point(20, 80),
            TextAlign = ContentAlignment.MiddleCenter
        };

        adminButton = new Button
        {
            Text = "  🔐  Admin Login",
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            BackColor = Color.FromArgb(79, 70, 229),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(280, 60),
            Location = new Point(50, 140),
            Cursor = Cursors.Hand
        };
        adminButton.FlatAppearance.BorderSize = 0;
        adminButton.Click += (s, e) => ShowAdminLogin();

        customerButton = new Button
        {
            Text = "  🛍️  Customer Portal",
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            BackColor = Color.FromArgb(16, 185, 129),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(280, 60),
            Location = new Point(50, 220),
            Cursor = Cursors.Hand
        };
        customerButton.FlatAppearance.BorderSize = 0;
        customerButton.Click += (s, e) => OpenCustomerPortal();

        // Admin login fields (hidden initially)
        var emailLabel = new Label
        {
            Text = "Email",
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            ForeColor = Color.FromArgb(100, 100, 100),
            Size = new Size(340, 20),
            Location = new Point(20, 120)
        };

        emailBox = new TextBox
        {
            Font = new Font("Segoe UI", 12),
            Size = new Size(340, 30),
            Location = new Point(20, 145),
            BorderStyle = BorderStyle.FixedSingle
        };

        var passwordLabel = new Label
        {
            Text = "Password",
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            ForeColor = Color.FromArgb(100, 100, 100),
            Size = new Size(340, 20),
            Location = new Point(20, 190)
        };

        passwordBox = new TextBox
        {
            Font = new Font("Segoe UI", 12),
            Size = new Size(340, 30),
            Location = new Point(20, 215),
            BorderStyle = BorderStyle.FixedSingle,
            PasswordChar = '*'
        };

        signInButton = new Button
        {
            Text = "Sign In",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            BackColor = Color.FromArgb(79, 70, 229),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(340, 42),
            Location = new Point(20, 265),
            Cursor = Cursors.Hand
        };
        signInButton.FlatAppearance.BorderSize = 0;
        signInButton.Click += AdminSignIn_Click;

        errorLabel = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            ForeColor = Color.Red,
            Size = new Size(340, 20),
            Location = new Point(20, 315),
            TextAlign = ContentAlignment.MiddleCenter
        };

        backButton = new Button
        {
            Text = "← Back",
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            ForeColor = Color.FromArgb(100, 100, 100),
            BackColor = Color.Transparent,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(80, 30),
            Location = new Point(10, 10),
            Cursor = Cursors.Hand
        };
        backButton.FlatAppearance.BorderSize = 0;
        backButton.Click += (s, e) => ShowPortalSelection();

        adminPanel = new Panel
        {
            Size = new Size(380, 370),
            Location = new Point(0, 0),
            BackColor = Color.White,
            Visible = false
        };
        adminPanel.Controls.AddRange([emailLabel, emailBox, passwordLabel, passwordBox, signInButton, errorLabel, backButton]);

        cardPanel.Controls.AddRange([titleLabel, subtitle, adminButton, customerButton, adminPanel]);
        this.Controls.Add(cardPanel);

        this.Resize += (s, e) =>
        {
            cardPanel.Location = new Point((this.ClientSize.Width - 380) / 2, (this.ClientSize.Height - 370) / 2);
        };
    }

    private void ShowAdminLogin()
    {
        titleLabel.Text = "Admin Login";
        subtitle.Visible = false;
        adminButton.Visible = false;
        customerButton.Visible = false;
        adminPanel.Visible = true;
        emailBox.Text = "";
        passwordBox.Text = "";
        errorLabel.Text = "";
    }

    private void ShowPortalSelection()
    {
        titleLabel.Text = "Justine Apparel";
        adminPanel.Visible = false;
        subtitle.Visible = true;
        adminButton.Visible = true;
        customerButton.Visible = true;
        errorLabel.Text = "";
    }

    private void OpenCustomerPortal()
    {
        var custLogin = new CustomerLoginForm();
        custLogin.FormClosed += (_, _) => this.Show();
        custLogin.Show();
        this.Hide();
    }

    private void AdminSignIn_Click(object? sender, EventArgs e)
    {
        var email = emailBox.Text.Trim();
        var password = passwordBox.Text;

        if (!email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
        {
            errorLabel.Text = "Email must be a Gmail address (@gmail.com).";
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            errorLabel.Text = "Password cannot be empty.";
            return;
        }

        var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, IsAdmin FROM Users WHERE Email = @email AND Password = @password";
        cmd.Parameters.AddWithValue("@email", email.ToLower());
        cmd.Parameters.AddWithValue("@password", password);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            var isAdmin = reader.GetInt32(2) == 1;
            if (isAdmin)
            {
                var userName = reader.GetString(1);
                var dashboard = new Dashboard(email, userName);
                dashboard.Show();
                this.Hide();
            }
            else
            {
                errorLabel.Text = "Access denied. Admin only.";
            }
        }
        else
        {
            errorLabel.Text = "Invalid email or password.";
        }
    }
}
