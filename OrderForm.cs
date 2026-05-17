namespace Justine_Apparel_E_Commerce_System;

public class OrderForm : Form
{
    private DataGridView grid;
    private Button refreshBtn, viewItemsBtn, filterBtn, statusBtn;
    private DateTimePicker dtpFrom, dtpTo;
    private Label revenueLabel;

    public OrderForm()
    {
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        this.Text = "Order Management";
        this.Size = new Size(1000, 650);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(240, 242, 245);

        var topPanel = new Panel
        {
            Height = 100,
            Dock = DockStyle.Top,
            BackColor = Color.White
        };

        var title = new Label
        {
            Text = "Orders",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 30),
            Location = new Point(15, 12),
            AutoSize = true
        };

        var fromLabel = new Label
        {
            Text = "From:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(100, 100, 100),
            Location = new Point(15, 50),
            AutoSize = true
        };

        dtpFrom = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Location = new Point(55, 47),
            Size = new Size(130, 25),
            Font = new Font("Segoe UI", 10)
        };

        var toLabel = new Label
        {
            Text = "To:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(100, 100, 100),
            Location = new Point(195, 50),
            AutoSize = true
        };

        dtpTo = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Location = new Point(220, 47),
            Size = new Size(130, 25),
            Font = new Font("Segoe UI", 10),
            Value = DateTime.Today.AddDays(1)
        };

        filterBtn = CreateButton("Filter", Color.FromArgb(79, 70, 229), 365, 45);
        filterBtn.Click += (s, e) => LoadData();

        refreshBtn = CreateButton("Reset", Color.FromArgb(100, 100, 100), 490, 45);
        refreshBtn.Click += (s, e) =>
        {
            dtpFrom.Value = dtpFrom.MinDate;
            dtpTo.Value = DateTime.Today.AddDays(1);
            LoadData();
        };

        viewItemsBtn = CreateButton("View Items", Color.FromArgb(16, 185, 129), 605, 45);
        viewItemsBtn.Click += (s, e) => ViewOrderItems();

        statusBtn = CreateButton("Change Status", Color.FromArgb(245, 158, 11), 730, 45);
        statusBtn.Click += (s, e) => ChangeStatus();

        revenueLabel = new Label
        {
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(79, 70, 229),
            Location = new Point(855, 50),
            AutoSize = true
        };

        topPanel.Controls.AddRange([title, fromLabel, dtpFrom, toLabel, dtpTo,
            filterBtn, refreshBtn, viewItemsBtn, statusBtn, revenueLabel]);

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

        this.Controls.AddRange([grid, topPanel]);
    }

    private Button CreateButton(string text, Color color, int x, int y)
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

    private void LoadData()
    {
        var conn = Database.GetConnection();

        var fromDate = dtpFrom.Value.Date;
        var toDate = dtpTo.Value.Date.AddDays(1);

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                SELECT Id, CustomerId, CustomerName, OrderDate, Total, Status
                FROM Orders
                WHERE OrderDate >= @from AND OrderDate < @to
                ORDER BY OrderDate DESC";
            cmd.Parameters.AddWithValue("@from", fromDate);
            cmd.Parameters.AddWithValue("@to", toDate);
            var dt = new System.Data.DataTable();
            using var reader = cmd.ExecuteReader();
            dt.Load(reader);
            grid.DataSource = dt;
        }

        if (grid.Columns.Contains("Id"))
            grid.Columns["Id"].Visible = false;
        if (grid.Columns.Contains("CustomerId"))
            grid.Columns["CustomerId"].Visible = false;

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                SELECT ISNULL(COUNT(*), 0), ISNULL(SUM(Total), 0)
                FROM Orders
                WHERE OrderDate >= @from AND OrderDate < @to";
            cmd.Parameters.AddWithValue("@from", fromDate);
            cmd.Parameters.AddWithValue("@to", toDate);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var count = reader.GetInt32(0);
                var total = reader.GetDecimal(1);
                revenueLabel.Text = $"{count} orders  |  Total: ₱{total:N2}";
            }
        }
    }

    private void ViewOrderItems()
    {
        if (grid.CurrentRow == null) return;
        var orderId = (int)(grid.CurrentRow.Cells["Id"].Value ?? 0);
        var customerId = (int)(grid.CurrentRow.Cells["CustomerId"].Value ?? 0);
        var customerName = grid.CurrentRow.Cells["CustomerName"].Value?.ToString() ?? "";

        var detailForm = new Form
        {
            Text = $"Order #{orderId} - {customerName}",
            Size = new Size(650, 500),
            StartPosition = FormStartPosition.CenterParent,
            BackColor = Color.White
        };

        var conn = Database.GetConnection();

        var customerInfo = new Label
        {
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(50, 50, 50),
            BackColor = Color.FromArgb(240, 242, 245),
            Size = new Size(620, 70),
            Location = new Point(10, 10),
            Padding = new Padding(10),
            Text = ""
        };

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT Email, Phone, Address FROM Customers WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", customerId);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var email = reader.IsDBNull(0) ? "N/A" : reader.GetString(0);
                var phone = reader.IsDBNull(1) ? "N/A" : reader.GetString(1);
                var address = reader.IsDBNull(2) ? "N/A" : reader.GetString(2);
                customerInfo.Text = $"Customer: {customerName}\nEmail: {email}  |  Phone: {phone}\nAddress: {address}";
            }
        }

        var itemsGrid = new DataGridView
        {
            Location = new Point(10, 90),
            Size = new Size(610, 340),
            AllowUserToAddRows = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Font = new Font("Segoe UI", 10)
        };
        itemsGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 242, 245);
        itemsGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

        using var itemsCmd = conn.CreateCommand();
        itemsCmd.CommandText = "SELECT ProductName, Quantity, Price FROM OrderItems WHERE OrderId = @id";
        itemsCmd.Parameters.AddWithValue("@id", orderId);
        var dt = new System.Data.DataTable();
        using var itemsReader = itemsCmd.ExecuteReader();
        dt.Load(itemsReader);
        itemsGrid.DataSource = dt;

        detailForm.Controls.AddRange([customerInfo, itemsGrid]);
        detailForm.ShowDialog();
    }

    private void ChangeStatus()
    {
        if (grid.CurrentRow == null)
        {
            MessageBox.Show("Please select an order first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var orderId = (int)(grid.CurrentRow.Cells["Id"].Value ?? 0);
        var currentStatus = grid.CurrentRow.Cells["Status"].Value?.ToString() ?? "Pending";

        var statusForm = new Form
        {
            Text = "Change Order Status",
            Size = new Size(300, 200),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.White
        };

        var lbl = new Label
        {
            Text = $"Order #{orderId}\nCurrent: {currentStatus}",
            Font = new Font("Segoe UI", 11, FontStyle.Regular),
            ForeColor = Color.FromArgb(50, 50, 50),
            Size = new Size(260, 40),
            Location = new Point(15, 15),
            TextAlign = ContentAlignment.MiddleLeft
        };

        var statusBox = new ComboBox
        {
            Font = new Font("Segoe UI", 11),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Size = new Size(260, 28),
            Location = new Point(15, 65)
        };
        statusBox.Items.AddRange(["Pending", "Processing", "Shipped", "Delivered", "Cancelled"]);
        statusBox.SelectedItem = currentStatus;

        var saveBtn = new Button
        {
            Text = "Update",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(79, 70, 229),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(120, 35),
            Location = new Point(30, 110),
            Cursor = Cursors.Hand
        };
        saveBtn.FlatAppearance.BorderSize = 0;
        saveBtn.Click += (s, e) =>
        {
            var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Orders SET Status = @status WHERE Id = @id";
            cmd.Parameters.AddWithValue("@status", statusBox.Text);
            cmd.Parameters.AddWithValue("@id", orderId);
            cmd.ExecuteNonQuery();
            statusForm.Close();
            LoadData();
        };

        var cancelBtn = new Button
        {
            Text = "Cancel",
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            BackColor = Color.FromArgb(200, 200, 200),
            ForeColor = Color.FromArgb(50, 50, 50),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(100, 35),
            Location = new Point(160, 110),
            Cursor = Cursors.Hand,
            DialogResult = DialogResult.Cancel
        };
        cancelBtn.FlatAppearance.BorderSize = 0;

        statusForm.Controls.AddRange([lbl, statusBox, saveBtn, cancelBtn]);
        statusForm.ShowDialog();
    }
}
