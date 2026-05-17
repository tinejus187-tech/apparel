namespace Justine_Apparel_E_Commerce_System;

public class CustomerAccountForm : Form
{
    private TextBox nameBox, emailBox, phoneBox, addressBox;
    private Label ordersLabel, spentLabel;
    private Button saveBtn;
    private int customerId;

    public CustomerAccountForm(int cid)
    {
        customerId = cid;
        InitializeComponent();
        LoadAccount();
    }

    private void InitializeComponent()
    {
        this.Text = "My Account";
        this.Size = new Size(450, 480);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.White;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        var title = new Label
        {
            Text = "Account Details",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 30),
            Location = new Point(20, 15),
            AutoSize = true
        };

        int y = 55;
        (nameBox, y) = AddField("Name", y);
        (emailBox, y) = AddReadonlyField("Email", y);
        (phoneBox, y) = AddField("Phone", y);
        (addressBox, y) = AddMultilineField("Address", y);

        ordersLabel = new Label
        {
            Font = new Font("Segoe UI", 11, FontStyle.Regular),
            ForeColor = Color.FromArgb(80, 80, 80),
            Size = new Size(380, 25),
            Location = new Point(25, y),
            Text = "Total Orders: 0"
        };
        y += 30;

        spentLabel = new Label
        {
            Font = new Font("Segoe UI", 11, FontStyle.Regular),
            ForeColor = Color.FromArgb(80, 80, 80),
            Size = new Size(380, 25),
            Location = new Point(25, y),
            Text = "Total Spent: ₱0.00"
        };
        y += 40;

        saveBtn = new Button
        {
            Text = "Save Changes",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            BackColor = Color.FromArgb(79, 70, 229),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(380, 40),
            Location = new Point(25, y),
            Cursor = Cursors.Hand
        };
        saveBtn.FlatAppearance.BorderSize = 0;
        saveBtn.Click += SaveBtn_Click;

        this.Controls.AddRange([title, ordersLabel, spentLabel, saveBtn]);
    }

    private (TextBox, int) AddField(string label, int y)
    {
        var lbl = new Label
        {
            Text = label,
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            ForeColor = Color.FromArgb(100, 100, 100),
            Size = new Size(380, 20),
            Location = new Point(25, y)
        };
        var box = new TextBox
        {
            Font = new Font("Segoe UI", 11),
            Size = new Size(380, 25),
            Location = new Point(25, y + 22),
            BorderStyle = BorderStyle.FixedSingle
        };
        this.Controls.AddRange([lbl, box]);
        return (box, y + 55);
    }

    private (TextBox, int) AddReadonlyField(string label, int y)
    {
        var (box, ny) = AddField(label, y);
        box.ReadOnly = true;
        box.BackColor = Color.FromArgb(240, 242, 245);
        return (box, ny);
    }

    private (TextBox, int) AddMultilineField(string label, int y)
    {
        var lbl = new Label
        {
            Text = label,
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            ForeColor = Color.FromArgb(100, 100, 100),
            Size = new Size(380, 20),
            Location = new Point(25, y)
        };
        var box = new TextBox
        {
            Font = new Font("Segoe UI", 11),
            Size = new Size(380, 50),
            Location = new Point(25, y + 22),
            BorderStyle = BorderStyle.FixedSingle,
            Multiline = true
        };
        this.Controls.AddRange([lbl, box]);
        return (box, y + 80);
    }

    private void LoadAccount()
    {
        var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Name, Email, Phone, Address, Orders, Spent FROM Customers WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", customerId);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            nameBox.Text = reader.GetString(0);
            emailBox.Text = reader.GetString(1);
            phoneBox.Text = reader.IsDBNull(2) ? "" : reader.GetString(2);
            addressBox.Text = reader.IsDBNull(3) ? "" : reader.GetString(3);
            ordersLabel.Text = $"Total Orders: {reader.GetInt32(4)}";
            spentLabel.Text = $"Total Spent: ₱{reader.GetDecimal(5):N2}";
        }
    }

    private void SaveBtn_Click(object? sender, EventArgs e)
    {
        var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Customers SET Name = @name, Phone = @phone, Address = @address WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", customerId);
        cmd.Parameters.AddWithValue("@name", nameBox.Text.Trim());
        cmd.Parameters.AddWithValue("@phone", phoneBox.Text.Trim());
        cmd.Parameters.AddWithValue("@address", addressBox.Text.Trim());
        cmd.ExecuteNonQuery();

        MessageBox.Show("Account updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
