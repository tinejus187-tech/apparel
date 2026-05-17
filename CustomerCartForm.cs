namespace Justine_Apparel_E_Commerce_System;

public class CustomerCartForm : Form
{
    private DataGridView grid;
    private Button checkoutBtn, removeBtn, refreshBtn, increaseBtn, decreaseBtn;
    private Label totalLabel;
    private int customerId;
    private string customerName;

    public CustomerCartForm(int cid, string cname)
    {
        customerId = cid;
        customerName = cname;
        InitializeComponent();
        LoadCart();
    }

    private void InitializeComponent()
    {
        this.Text = "Shopping Cart";
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
            Text = "Your Cart",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 30),
            Location = new Point(15, 12),
            AutoSize = true
        };

        totalLabel = new Label
        {
            Text = "Total: ₱0.00",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(79, 70, 229),
            Location = new Point(500, 15),
            AutoSize = true
        };

        topPanel.Controls.AddRange([title, totalLabel]);

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

        var bottomPanel = new Panel
        {
            Height = 60,
            Dock = DockStyle.Bottom,
            BackColor = Color.White
        };

        removeBtn = CreateButton("Remove", Color.FromArgb(239, 68, 68), 15, 12);
        removeBtn.Click += (s, e) => RemoveItem();

        decreaseBtn = CreateButton("−", Color.FromArgb(245, 158, 11), 145, 12);
        decreaseBtn.Size = new Size(40, 35);
        decreaseBtn.Click += (s, e) => ChangeQuantity(-1);

        increaseBtn = CreateButton("+", Color.FromArgb(16, 185, 129), 195, 12);
        increaseBtn.Size = new Size(40, 35);
        increaseBtn.Click += (s, e) => ChangeQuantity(1);

        refreshBtn = CreateButton("Refresh", Color.FromArgb(100, 100, 100), 245, 12);
        refreshBtn.Click += (s, e) => LoadCart();

        checkoutBtn = CreateButton("Proceed to Checkout", Color.FromArgb(79, 70, 229), 500, 12);
        checkoutBtn.Size = new Size(180, 35);
        checkoutBtn.Click += (s, e) => Checkout();

        bottomPanel.Controls.AddRange([removeBtn, decreaseBtn, increaseBtn, refreshBtn, checkoutBtn]);

        this.Controls.AddRange([grid, bottomPanel, topPanel]);
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
            Size = new Size(120, 35),
            Location = new Point(x, y),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    private void LoadCart()
    {
        var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT ci.Id, ci.ProductName, ci.Category, ci.Size, ci.Color, ci.Price, ci.Quantity, p.Stock FROM CartItems ci JOIN Products p ON ci.ProductId = p.Id WHERE ci.CustomerId = @cid ORDER BY ci.ProductName";
        cmd.Parameters.AddWithValue("@cid", customerId);
        var dt = new System.Data.DataTable();
        using var reader = cmd.ExecuteReader();
        dt.Load(reader);
        grid.DataSource = dt;

        if (grid.Columns.Contains("Id"))
            grid.Columns["Id"].Visible = false;
        if (grid.Columns.Contains("Stock"))
            grid.Columns["Stock"].HeaderText = "Available";

        decimal total = 0;
        foreach (System.Data.DataRow row in dt.Rows)
            total += (decimal)row["Price"] * (int)row["Quantity"];

        totalLabel.Text = $"Total: ₱{total:N2}";
    }

    private void ChangeQuantity(int delta)
    {
        if (grid.CurrentRow == null) return;
        var id = (int)(grid.CurrentRow.Cells["Id"].Value ?? 0);
        var qty = (int)(grid.CurrentRow.Cells["Quantity"].Value ?? 1);
        var stock = (int)(grid.CurrentRow.Cells["Stock"].Value ?? 0);
        var newQty = qty + delta;

        if (newQty < 1)
        {
            RemoveItem();
            return;
        }

        if (newQty > stock)
        {
            MessageBox.Show($"Only {stock} available in stock.", "Stock Limit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE CartItems SET Quantity = @qty WHERE Id = @id";
        cmd.Parameters.AddWithValue("@qty", newQty);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
        LoadCart();
    }

    private void RemoveItem()
    {
        if (grid.CurrentRow == null) return;
        var id = (int)(grid.CurrentRow.Cells["Id"].Value ?? 0);
        var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM CartItems WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
        LoadCart();
    }

    private void Checkout()
    {
        var conn = Database.GetConnection();
        using var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM CartItems WHERE CustomerId = @cid";
        checkCmd.Parameters.AddWithValue("@cid", customerId);
        if (Convert.ToInt32(checkCmd.ExecuteScalar() ?? 0) == 0)
        {
            MessageBox.Show("Your cart is empty.", "Cart", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var checkout = new CustomerCheckoutForm(customerId, customerName);
        if (checkout.ShowDialog() == DialogResult.OK)
            LoadCart();
    }
}
