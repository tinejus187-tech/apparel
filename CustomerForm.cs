namespace Justine_Apparel_E_Commerce_System;

public class CustomerForm : Form
{
    private TextBox searchBox;
    private DataGridView grid;
    private Button addBtn, editBtn, deleteBtn, refreshBtn;

    public CustomerForm()
    {
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        this.Text = "Customer Management";
        this.Size = new Size(1000, 600);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(240, 242, 245);

        var topPanel = new Panel
        {
            Height = 60,
            Dock = DockStyle.Top,
            BackColor = Color.White
        };

        var title = new Label
        {
            Text = "Customers",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 30),
            Location = new Point(15, 12),
            AutoSize = true
        };

        searchBox = new TextBox
        {
            Font = new Font("Segoe UI", 11),
            Size = new Size(250, 30),
            Location = new Point(500, 15),
            BorderStyle = BorderStyle.FixedSingle
        };
        searchBox.TextChanged += (s, e) => LoadData(searchBox.Text.Trim());

        addBtn = CreateToolButton("Add Customer", Color.FromArgb(79, 70, 229), 15, 70);
        addBtn.Click += (s, e) => AddCustomer();

        editBtn = CreateToolButton("Edit Customer", Color.FromArgb(16, 185, 129), 140, 70);
        editBtn.Click += (s, e) => EditCustomer();

        deleteBtn = CreateToolButton("Delete", Color.FromArgb(239, 68, 68), 265, 70);
        deleteBtn.Click += (s, e) => DeleteCustomer();

        refreshBtn = CreateToolButton("Refresh", Color.FromArgb(100, 100, 100), 390, 70);
        refreshBtn.Click += (s, e) => LoadData();

        topPanel.Controls.AddRange([title, searchBox]);

        grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Font = new Font("Segoe UI", 10)
        };
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 242, 245);
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

        this.Controls.AddRange([grid, deleteBtn, editBtn, addBtn, refreshBtn, topPanel]);
    }

    private Button CreateToolButton(string text, Color color, int x, int y)
    {
        var btn = new Button
        {
            Text = text,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(115, 35),
            Location = new Point(x, y),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    private void LoadData(string search = "")
    {
        var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();

        if (string.IsNullOrEmpty(search))
            cmd.CommandText = "SELECT Id, Name, Email, Phone, Address, Orders, Spent, CONVERT(VARCHAR(16), JoinedDate, 120) AS JoinedDate FROM Customers ORDER BY Name";
        else
        {
            cmd.CommandText = "SELECT Id, Name, Email, Phone, Address, Orders, Spent, CONVERT(VARCHAR(16), JoinedDate, 120) AS JoinedDate FROM Customers WHERE Name LIKE @search OR Email LIKE @search ORDER BY Name";
            cmd.Parameters.AddWithValue("@search", $"%{search}%");
        }

        var dt = new System.Data.DataTable();
        using var reader = cmd.ExecuteReader();
        dt.Load(reader);
        grid.DataSource = dt;

        if (grid.Columns.Contains("Id"))
            grid.Columns["Id"].Visible = false;
    }

    private void AddCustomer()
    {
        var detail = new CustomerDetailForm();
        if (detail.ShowDialog() == DialogResult.OK)
        {
            var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Customers (Name, Email, Password, Phone, Address) VALUES (@name, @email, @password, @phone, @address)";
            cmd.Parameters.AddWithValue("@name", detail.CustomerName);
            cmd.Parameters.AddWithValue("@email", detail.CustomerEmail);
            cmd.Parameters.AddWithValue("@password", detail.CustomerPassword);
            cmd.Parameters.AddWithValue("@phone", detail.CustomerPhone);
            cmd.Parameters.AddWithValue("@address", detail.CustomerAddress);
            cmd.ExecuteNonQuery();
            LoadData();
        }
    }

    private void EditCustomer()
    {
        if (grid.CurrentRow == null) return;
        var id = (int)(grid.CurrentRow.Cells["Id"].Value ?? 0);

        var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Customers WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return;

        var detail = new CustomerDetailForm
        {
            CustomerName = reader.GetString(1),
            CustomerEmail = reader.GetString(2),
            CustomerPassword = reader.GetString(3),
            CustomerPhone = reader.IsDBNull(4) ? "" : reader.GetString(4),
            CustomerAddress = reader.IsDBNull(5) ? "" : reader.GetString(5)
        };

        if (detail.ShowDialog() == DialogResult.OK)
        {
            var conn2 = Database.GetConnection();
            using var cmd2 = conn2.CreateCommand();
            cmd2.CommandText = "UPDATE Customers SET Name = @name, Email = @email, Password = @password, Phone = @phone, Address = @address WHERE Id = @id";
            cmd2.Parameters.AddWithValue("@id", id);
            cmd2.Parameters.AddWithValue("@name", detail.CustomerName);
            cmd2.Parameters.AddWithValue("@email", detail.CustomerEmail);
            cmd2.Parameters.AddWithValue("@password", detail.CustomerPassword);
            cmd2.Parameters.AddWithValue("@phone", detail.CustomerPhone);
            cmd2.Parameters.AddWithValue("@address", detail.CustomerAddress);
            cmd2.ExecuteNonQuery();
            LoadData();
        }
    }

    private void DeleteCustomer()
    {
        if (grid.CurrentRow == null) return;
        var id = (int)(grid.CurrentRow.Cells["Id"].Value ?? 0);
        var name = grid.CurrentRow.Cells["Name"].Value?.ToString() ?? "";

        if (MessageBox.Show($"Delete customer '{name}'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
        {
            var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Customers WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            LoadData();
        }
    }
}

public class CustomerDetailForm : Form
{
    private TextBox nameBox, emailBox, passwordBox, phoneBox, addressBox;
    private Button saveBtn, cancelBtn;

    public string CustomerName
    {
        get => nameBox.Text.Trim();
        set => nameBox.Text = value;
    }
    public string CustomerEmail
    {
        get => emailBox.Text.Trim();
        set => emailBox.Text = value;
    }
    public string CustomerPassword
    {
        get => passwordBox.Text;
        set => passwordBox.Text = value;
    }
    public string CustomerPhone
    {
        get => phoneBox.Text.Trim();
        set => phoneBox.Text = value;
    }
    public string CustomerAddress
    {
        get => addressBox.Text.Trim();
        set => addressBox.Text = value;
    }

    public CustomerDetailForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Customer Details";
        this.Size = new Size(450, 440);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.White;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        int y = 20;
        (nameBox, y) = AddField("Name", y);
        (emailBox, y) = AddField("Email", y);
        (passwordBox, y) = AddPasswordField("Password", y);
        (phoneBox, y) = AddField("Phone", y);
        (addressBox, y) = AddField("Address", y);

        saveBtn = new Button
        {
            Text = "Save",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            BackColor = Color.FromArgb(79, 70, 229),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(120, 35),
            Location = new Point(90, y + 10),
            Cursor = Cursors.Hand,
            DialogResult = DialogResult.OK
        };
        saveBtn.FlatAppearance.BorderSize = 0;

        cancelBtn = new Button
        {
            Text = "Cancel",
            Font = new Font("Segoe UI", 11, FontStyle.Regular),
            BackColor = Color.FromArgb(200, 200, 200),
            ForeColor = Color.FromArgb(50, 50, 50),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(120, 35),
            Location = new Point(230, y + 10),
            Cursor = Cursors.Hand,
            DialogResult = DialogResult.Cancel
        };
        cancelBtn.FlatAppearance.BorderSize = 0;

        this.Controls.AddRange([saveBtn, cancelBtn]);
    }

    private (TextBox, int) AddPasswordField(string label, int y)
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
            BorderStyle = BorderStyle.FixedSingle,
            PasswordChar = '*'
        };
        this.Controls.AddRange([lbl, box]);
        return (box, y + 60);
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
        return (box, y + 60);
    }
}
