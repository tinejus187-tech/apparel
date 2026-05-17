namespace Justine_Apparel_E_Commerce_System;

public class CustomerOrdersForm : Form
{
    private DataGridView grid;
    private Button viewItemsBtn;
    private int customerId;

    public CustomerOrdersForm(int cid)
    {
        customerId = cid;
        InitializeComponent();
        LoadOrders();
    }

    private void InitializeComponent()
    {
        this.Text = "My Orders";
        this.Size = new Size(800, 500);
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
            Text = "Order History",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 30),
            Location = new Point(15, 12),
            AutoSize = true
        };

        viewItemsBtn = new Button
        {
            Text = "View Items",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(79, 70, 229),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(120, 35),
            Location = new Point(600, 12),
            Cursor = Cursors.Hand
        };
        viewItemsBtn.FlatAppearance.BorderSize = 0;
        viewItemsBtn.Click += (s, e) => ViewOrderItems();

        topPanel.Controls.AddRange([title, viewItemsBtn]);

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

    private void LoadOrders()
    {
        var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, OrderDate, Total, Status FROM Orders WHERE CustomerId = @cid ORDER BY OrderDate DESC";
        cmd.Parameters.AddWithValue("@cid", customerId);
        var dt = new System.Data.DataTable();
        using var reader = cmd.ExecuteReader();
        dt.Load(reader);
        grid.DataSource = dt;

        if (grid.Columns.Contains("Id"))
            grid.Columns["Id"].Visible = false;
    }

    private void ViewOrderItems()
    {
        if (grid.CurrentRow == null) return;
        var orderId = (int)(grid.CurrentRow.Cells["Id"].Value ?? 0);
        var date = grid.CurrentRow.Cells["OrderDate"].Value?.ToString() ?? "";

        var detailForm = new Form
        {
            Text = $"Order #{orderId} - {date}",
            Size = new Size(500, 350),
            StartPosition = FormStartPosition.CenterParent,
            BackColor = Color.White
        };

        var itemsGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Font = new Font("Segoe UI", 10)
        };
        itemsGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 242, 245);
        itemsGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

        var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT ProductName, Quantity, Price FROM OrderItems WHERE OrderId = @id";
        cmd.Parameters.AddWithValue("@id", orderId);
        var dt = new System.Data.DataTable();
        using var reader = cmd.ExecuteReader();
        dt.Load(reader);
        itemsGrid.DataSource = dt;

        detailForm.Controls.Add(itemsGrid);
        detailForm.ShowDialog();
    }
}
