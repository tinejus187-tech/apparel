namespace Justine_Apparel_E_Commerce_System;

public class CustomerCheckoutForm : Form
{
    private DataGridView grid;
    private Label totalLabel;
    private Label addressLabel;
    private TextBox addressBox;
    private Button placeOrderBtn, cancelBtn;
    private int customerId;
    private string customerName;

    public CustomerCheckoutForm(int cid, string cname)
    {
        customerId = cid;
        customerName = cname;
        InitializeComponent();
        LoadCartItems();
    }

    private void InitializeComponent()
    {
        this.Text = "Checkout";
        this.Size = new Size(600, 500);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.White;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        var title = new Label
        {
            Text = "Order Summary",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 30),
            Location = new Point(15, 15),
            AutoSize = true
        };

        grid = new DataGridView
        {
            Size = new Size(555, 200),
            Location = new Point(15, 50),
            AllowUserToAddRows = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Font = new Font("Segoe UI", 10)
        };
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 242, 245);
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

        totalLabel = new Label
        {
            Text = "Total: ₱0.00",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(79, 70, 229),
            Location = new Point(15, 260),
            AutoSize = true
        };

        addressLabel = new Label
        {
            Text = "Delivery Address:",
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            ForeColor = Color.FromArgb(100, 100, 100),
            Size = new Size(555, 20),
            Location = new Point(15, 300)
        };

        addressBox = new TextBox
        {
            Font = new Font("Segoe UI", 11),
            Size = new Size(555, 25),
            Location = new Point(15, 325),
            BorderStyle = BorderStyle.FixedSingle,
            Multiline = true,
            Height = 50
        };

        LoadCustomerAddress();

        placeOrderBtn = new Button
        {
            Text = "Place Order",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            BackColor = Color.FromArgb(16, 185, 129),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(180, 42),
            Location = new Point(190, 400),
            Cursor = Cursors.Hand
        };
        placeOrderBtn.FlatAppearance.BorderSize = 0;
        placeOrderBtn.Click += PlaceOrder_Click;

        cancelBtn = new Button
        {
            Text = "Cancel",
            Font = new Font("Segoe UI", 11, FontStyle.Regular),
            BackColor = Color.FromArgb(200, 200, 200),
            ForeColor = Color.FromArgb(50, 50, 50),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(120, 42),
            Location = new Point(380, 400),
            Cursor = Cursors.Hand,
            DialogResult = DialogResult.Cancel
        };
        cancelBtn.FlatAppearance.BorderSize = 0;

        this.Controls.AddRange([title, grid, totalLabel, addressLabel, addressBox, placeOrderBtn, cancelBtn]);
    }

    private void LoadCustomerAddress()
    {
        var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Address FROM Customers WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", customerId);
        var addr = cmd.ExecuteScalar()?.ToString() ?? "";
        addressBox.Text = addr;
    }

    private void LoadCartItems()
    {
        var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT ProductName, Category, Size, Color, Price, Quantity FROM CartItems WHERE CustomerId = @cid";
        cmd.Parameters.AddWithValue("@cid", customerId);
        var dt = new System.Data.DataTable();
        using var reader = cmd.ExecuteReader();
        dt.Load(reader);
        grid.DataSource = dt;

        decimal total = 0;
        foreach (System.Data.DataRow row in dt.Rows)
            total += (decimal)row["Price"] * (int)row["Quantity"];

        totalLabel.Text = $"Total: ₱{total:N2}";
    }

    private void PlaceOrder_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(addressBox.Text))
        {
            MessageBox.Show("Please enter a delivery address.", "Address Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var conn = Database.GetConnection();

        using var stockCheckCmd = conn.CreateCommand();
        stockCheckCmd.CommandText = @"SELECT ci.ProductName, ci.Quantity, p.Stock FROM CartItems ci JOIN Products p ON ci.ProductId = p.Id WHERE ci.CustomerId = @cid AND ci.Quantity > p.Stock";
        stockCheckCmd.Parameters.AddWithValue("@cid", customerId);
        using var stockReader = stockCheckCmd.ExecuteReader();
        var outOfStock = new List<string>();
        while (stockReader.Read())
        {
            var name = stockReader.GetString(0);
            var qty = stockReader.GetInt32(1);
            var stock = stockReader.GetInt32(2);
            outOfStock.Add($"{name} (ordered {qty}, only {stock} in stock)");
        }
        stockReader.Close();

        if (outOfStock.Count > 0)
        {
            MessageBox.Show("Some items exceed available stock:\n\n" + string.Join("\n", outOfStock), "Insufficient Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var tx = conn.BeginTransaction();

        try
        {
            using var totalCmd = conn.CreateCommand();
            totalCmd.Transaction = tx;
            totalCmd.CommandText = "SELECT ISNULL(SUM(Price * Quantity), 0) FROM CartItems WHERE CustomerId = @cid";
            totalCmd.Parameters.AddWithValue("@cid", customerId);
            var total = Convert.ToDecimal(totalCmd.ExecuteScalar() ?? 0);

            using var orderCmd = conn.CreateCommand();
            orderCmd.Transaction = tx;
            orderCmd.CommandText = "INSERT INTO Orders (CustomerId, CustomerName, Total, Status) VALUES (@cid, @cname, @total, 'Pending'); SELECT CAST(SCOPE_IDENTITY() AS INT);";
            orderCmd.Parameters.AddWithValue("@cid", customerId);
            orderCmd.Parameters.AddWithValue("@cname", customerName);
            orderCmd.Parameters.AddWithValue("@total", total);
            var orderId = Convert.ToInt32(orderCmd.ExecuteScalar() ?? 0);

            using var itemsCmd = conn.CreateCommand();
            itemsCmd.Transaction = tx;
            itemsCmd.CommandText = "INSERT INTO OrderItems (OrderId, ProductId, ProductName, Quantity, Price) SELECT @oid, ProductId, ProductName, Quantity, Price FROM CartItems WHERE CustomerId = @cid";
            itemsCmd.Parameters.AddWithValue("@oid", orderId);
            itemsCmd.Parameters.AddWithValue("@cid", customerId);
            itemsCmd.ExecuteNonQuery();

            using var delCmd = conn.CreateCommand();
            delCmd.Transaction = tx;
            delCmd.CommandText = "DELETE FROM CartItems WHERE CustomerId = @cid";
            delCmd.Parameters.AddWithValue("@cid", customerId);
            delCmd.ExecuteNonQuery();

            using var updateStockCmd = conn.CreateCommand();
            updateStockCmd.Transaction = tx;
            updateStockCmd.CommandText = @"UPDATE Products SET Stock = Stock - (SELECT ISNULL(SUM(Quantity), 0) FROM OrderItems WHERE OrderItems.ProductId = Products.Id AND OrderItems.OrderId = @oid) WHERE Id IN (SELECT ProductId FROM OrderItems WHERE OrderId = @oid)";
            updateStockCmd.Parameters.AddWithValue("@oid", orderId);
            updateStockCmd.ExecuteNonQuery();

            using var updateCustCmd = conn.CreateCommand();
            updateCustCmd.Transaction = tx;
            updateCustCmd.CommandText = "UPDATE Customers SET Orders = Orders + 1, Spent = Spent + @total, Address = @addr WHERE Id = @cid";
            updateCustCmd.Parameters.AddWithValue("@total", total);
            updateCustCmd.Parameters.AddWithValue("@addr", addressBox.Text.Trim());
            updateCustCmd.Parameters.AddWithValue("@cid", customerId);
            updateCustCmd.ExecuteNonQuery();

            tx.Commit();

            MessageBox.Show($"Order placed successfully!\nOrder #: {orderId}\nTotal: ₱{total:N2}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
