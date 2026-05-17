using Microsoft.Data.SqlClient;

namespace Justine_Apparel_E_Commerce_System;

public class CustomerLoginForm : Form
{
    private Panel cardPanel;
    private Label titleLabel;
    private Label emailLabel;
    private TextBox emailBox;
    private Label passwordLabel;
    private TextBox passwordBox;
    private Button loginButton;
    private Button registerButton;
    private Label errorLabel = null!;
    private Label switchLabel;
    private bool isRegisterMode;

    public CustomerLoginForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Justine Apparel - Customer Portal";
        this.Size = new Size(500, 550);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(240, 242, 245);
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;

        cardPanel = new Panel
        {
            Size = new Size(380, 400),
            Location = new Point((this.ClientSize.Width - 380) / 2, (this.ClientSize.Height - 400) / 2),
            BackColor = Color.White,
            Anchor = AnchorStyles.None
        };

        titleLabel = new Label
        {
            Text = "Customer Login",
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 30),
            Size = new Size(340, 40),
            Location = new Point(20, 30),
            TextAlign = ContentAlignment.MiddleLeft
        };

        emailLabel = new Label
        {
            Text = "Email",
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            ForeColor = Color.FromArgb(100, 100, 100),
            Size = new Size(340, 20),
            Location = new Point(20, 90)
        };

        emailBox = new TextBox
        {
            Font = new Font("Segoe UI", 12),
            Size = new Size(340, 30),
            Location = new Point(20, 115),
            BorderStyle = BorderStyle.FixedSingle
        };

        passwordLabel = new Label
        {
            Text = "Password",
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            ForeColor = Color.FromArgb(100, 100, 100),
            Size = new Size(340, 20),
            Location = new Point(20, 160)
        };

        passwordBox = new TextBox
        {
            Font = new Font("Segoe UI", 12),
            Size = new Size(340, 30),
            Location = new Point(20, 185),
            BorderStyle = BorderStyle.FixedSingle,
            PasswordChar = '*'
        };

        loginButton = new Button
        {
            Text = "Sign In",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            BackColor = Color.FromArgb(79, 70, 229),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(340, 42),
            Location = new Point(20, 240),
            Cursor = Cursors.Hand
        };
        loginButton.FlatAppearance.BorderSize = 0;
        loginButton.Click += LoginButton_Click;

        registerButton = new Button
        {
            Text = "Create Account",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            BackColor = Color.FromArgb(16, 185, 129),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(340, 42),
            Location = new Point(20, 290),
            Cursor = Cursors.Hand
        };
        registerButton.FlatAppearance.BorderSize = 0;
        registerButton.Click += RegisterButton_Click;

        errorLabel = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            ForeColor = Color.Red,
            Size = new Size(340, 20),
            Location = new Point(20, 340),
            TextAlign = ContentAlignment.MiddleCenter
        };

        switchLabel = new Label
        {
            Text = "Don't have an account? Register here.",
            Font = new Font("Segoe UI", 9, FontStyle.Underline),
            ForeColor = Color.FromArgb(79, 70, 229),
            Size = new Size(340, 20),
            Location = new Point(20, 365),
            TextAlign = ContentAlignment.MiddleCenter,
            Cursor = Cursors.Hand
        };
        switchLabel.Click += (s, e) => SwitchMode();

        cardPanel.Controls.AddRange([titleLabel, emailLabel, emailBox, passwordLabel, passwordBox, loginButton, registerButton, errorLabel, switchLabel]);
        this.Controls.Add(cardPanel);

        this.Resize += (s, e) =>
        {
            cardPanel.Location = new Point((this.ClientSize.Width - 380) / 2, (this.ClientSize.Height - 400) / 2);
        };
    }

    private void SwitchMode()
    {
        isRegisterMode = !isRegisterMode;
        titleLabel.Text = isRegisterMode ? "Create Account" : "Customer Login";
        loginButton.Text = isRegisterMode ? "Register & Sign In" : "Sign In";
        registerButton.Visible = !isRegisterMode;
        switchLabel.Text = isRegisterMode ? "Already have an account? Sign in." : "Don't have an account? Register here.";
        errorLabel.Text = "";
    }

    private void LoginButton_Click(object? sender, EventArgs e)
    {
        if (isRegisterMode)
        {
            RegisterCustomer();
            return;
        }

        var email = emailBox.Text.Trim().ToLower();
        var password = passwordBox.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            errorLabel.Text = "Please fill in all fields.";
            return;
        }

        var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name FROM Customers WHERE LOWER(Email) = @email AND Password = @password";
        cmd.Parameters.AddWithValue("@email", email);
        cmd.Parameters.AddWithValue("@password", password);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            var id = reader.GetInt32(0);
            var name = reader.GetString(1);
            var portal = new CustomerPortalForm(id, name, email);
            portal.Show();
            this.Hide();
        }
        else
        {
            errorLabel.Text = "Invalid email or password.";
        }
    }

    private void RegisterButton_Click(object? sender, EventArgs e)
    {
        SwitchMode();
    }

    private void RegisterCustomer()
    {
        var email = emailBox.Text.Trim().ToLower();
        var password = passwordBox.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            errorLabel.Text = "Please fill in all fields.";
            return;
        }

        if (!email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
        {
            errorLabel.Text = "Email must be a Gmail address (@gmail.com).";
            return;
        }

        if (password.Length < 4)
        {
            errorLabel.Text = "Password must be at least 4 characters.";
            return;
        }

        try
        {
            var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Customers (Name, Email, Password) VALUES (@name, @email, @password)";
            cmd.Parameters.AddWithValue("@name", email.Split('@')[0]);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@password", password);
            cmd.ExecuteNonQuery();

            MessageBox.Show("Account created successfully! You can now sign in.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            SwitchMode();
            emailBox.Text = email;
            passwordBox.Text = "";
        }
        catch (SqlException)
        {
            errorLabel.Text = "An account with this email already exists.";
        }
    }
}
