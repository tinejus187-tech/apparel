namespace Justine_Apparel_E_Commerce_System;

public class CustomerPortalForm : Form
{
    private Panel headerPanel;
    private Panel contentPanel;
    private FlowLayoutPanel productsPanel;
    private Label cartBadge;
    private int customerId;
    private string customerName;
    private string customerEmail;
    private ComboBox categoryFilter;
    private TextBox searchBox;

    public CustomerPortalForm(int id, string name, string email)
    {
        customerId = id;
        customerName = name;
        customerEmail = email;
        InitializeComponent();
        this.Activated += (s, e) => { if (productsPanel.Controls.Count > 0) LoadProducts(); };
        LoadProducts();
    }

    private void InitializeComponent()
    {
        this.Text = "Justine Apparel - Shop";
        this.WindowState = FormWindowState.Maximized;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(240, 242, 245);

        headerPanel = new Panel
        {
            Height = 70,
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(30, 30, 40)
        };

        var logo = new Label
        {
            Text = "Justine Apparel",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(20, 18),
            AutoSize = true
        };

        var welcome = new Label
        {
            Text = $"Welcome, {customerName}!",
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            ForeColor = Color.FromArgb(180, 180, 190),
            Location = new Point(20, 45),
            AutoSize = true
        };

        var btnCart = new Button
        {
            Text = "  🛒  Cart",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(79, 70, 229),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(120, 40),
            Location = new Point(headerPanel.Width - 380, 15),
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnCart.FlatAppearance.BorderSize = 0;
        btnCart.Click += (s, e) => OpenCart();

        cartBadge = new Label
        {
            Text = "0",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Red,
            Size = new Size(22, 22),
            Location = new Point(btnCart.Right - 30, btnCart.Top - 5),
            TextAlign = ContentAlignment.MiddleCenter,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        var btnOrders = new Button
        {
            Text = "  📋  Orders",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(120, 40),
            Location = new Point(headerPanel.Width - 250, 15),
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnOrders.FlatAppearance.BorderSize = 0;
        btnOrders.Click += (s, e) => OpenOrders();

        var btnAccount = new Button
        {
            Text = "  👤  Account",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(120, 40),
            Location = new Point(headerPanel.Width - 120, 15),
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnAccount.FlatAppearance.BorderSize = 0;
        btnAccount.Click += (s, e) => OpenAccount();

        var btnLogout = new Button
        {
            Text = "Logout",
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            ForeColor = Color.FromArgb(200, 100, 100),
            BackColor = Color.Transparent,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(80, 30),
            Location = new Point(20, 0),
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left
        };
        btnLogout.FlatAppearance.BorderSize = 0;
        btnLogout.Click += (s, e) => Logout();

        headerPanel.Controls.AddRange([logo, welcome, btnCart, cartBadge, btnOrders, btnAccount, btnLogout]);
        headerPanel.Resize += (s, e) =>
        {
            btnCart.Location = new Point(headerPanel.Width - 380, 15);
            cartBadge.Location = new Point(btnCart.Right - 30, btnCart.Top - 5);
            btnOrders.Location = new Point(headerPanel.Width - 250, 15);
            btnAccount.Location = new Point(headerPanel.Width - 120, 15);
        };

        var filterPanel = new Panel
        {
            Height = 60,
            Dock = DockStyle.Top,
            BackColor = Color.White
        };

        var shopTitle = new Label
        {
            Text = "Our Products",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 30),
            Location = new Point(20, 14),
            AutoSize = true
        };

        categoryFilter = new ComboBox
        {
            Font = new Font("Segoe UI", 11),
            Size = new Size(150, 30),
            Location = new Point(200, 14),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        categoryFilter.Items.AddRange(["All Categories", "Tops", "Bottoms", "Dresses", "Outerwear", "Accessories"]);
        categoryFilter.SelectedIndex = 0;
        categoryFilter.SelectedIndexChanged += (s, e) => LoadProducts();

        searchBox = new TextBox
        {
            Font = new Font("Segoe UI", 11),
            Size = new Size(200, 30),
            Location = new Point(370, 14),
            BorderStyle = BorderStyle.FixedSingle
        };
        searchBox.TextChanged += (s, e) => LoadProducts();

        filterPanel.Controls.AddRange([shopTitle, categoryFilter, searchBox]);

        productsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.FromArgb(240, 242, 245),
            Padding = new Padding(20)
        };

        contentPanel = new Panel
        {
            Dock = DockStyle.Fill
        };
        contentPanel.Controls.AddRange([productsPanel, filterPanel]);

        this.Controls.AddRange([contentPanel, headerPanel]);
        UpdateCartBadge();
    }

    private void LoadProducts()
    {
        productsPanel.Controls.Clear();

        var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();

        var sql = "SELECT Id, ProductName, Category, Size, Color, Price, Stock, Status, ImagePath FROM Products WHERE Status = 'Active'";
        var conditions = new List<string>();
        var search = searchBox.Text.Trim();
        var category = categoryFilter.SelectedItem?.ToString();

        if (!string.IsNullOrEmpty(search))
        {
            conditions.Add("(ProductName LIKE @search OR Category LIKE @search)");
            cmd.Parameters.AddWithValue("@search", $"%{search}%");
        }
        if (category != null && category != "All Categories")
        {
            conditions.Add("Category = @cat");
            cmd.Parameters.AddWithValue("@cat", category);
        }

        if (conditions.Count > 0)
            sql += " AND " + string.Join(" AND ", conditions);

        cmd.CommandText = sql + " ORDER BY ProductName";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var id = reader.GetInt32(0);
            var pName = reader.GetString(1);
            var cat = reader.GetString(2);
            var size = reader.IsDBNull(3) ? "" : reader.GetString(3);
            var color = reader.IsDBNull(4) ? "" : reader.GetString(4);
            var price = reader.GetDecimal(5);
            var stock = reader.GetInt32(6);
            var imgPath = reader.IsDBNull(8) ? null : reader.GetString(8);

            productsPanel.Controls.Add(CreateProductCard(id, pName, cat, size, color, price, stock, imgPath));
        }
    }

    private Panel CreateProductCard(int id, string name, string category, string size, string color, decimal price, int stock, string? imagePath)
    {
        var card = new Panel
        {
            Size = new Size(220, 320),
            BackColor = Color.White,
            Margin = new Padding(10)
        };

        var picBox = new PictureBox
        {
            Size = new Size(200, 140),
            Location = new Point(10, 10),
            BackColor = Color.FromArgb(240, 242, 245),
            SizeMode = PictureBoxSizeMode.Zoom
        };

        if (!string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath))
        {
            try { picBox.Image = Image.FromFile(imagePath); }
            catch { picBox.BackColor = Color.FromArgb(240, 242, 245); }
        }
        else
        {
            picBox.BackColor = Color.FromArgb(230, 230, 235);
        }

        var nameLbl = new Label
        {
            Text = name,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 30),
            Size = new Size(200, 22),
            Location = new Point(10, 158)
        };

        var details = $"{category}";
        if (!string.IsNullOrEmpty(size)) details += $" | {size}";
        if (!string.IsNullOrEmpty(color)) details += $" | {color}";

        var infoLbl = new Label
        {
            Text = details,
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            ForeColor = Color.FromArgb(120, 120, 120),
            Size = new Size(200, 18),
            Location = new Point(10, 180)
        };

        var priceLbl = new Label
        {
            Text = $"₱{price:N2}",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(79, 70, 229),
            Size = new Size(200, 25),
            Location = new Point(10, 205)
        };

        var stockLbl = new Label
        {
            Text = stock > 0 ? $"In Stock: {stock}" : "Out of Stock",
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            ForeColor = stock > 0 ? Color.FromArgb(16, 185, 129) : Color.Red,
            Size = new Size(200, 18),
            Location = new Point(10, 232)
        };

        var addBtn = new Button
        {
            Text = "Add to Cart",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = stock > 0 ? Color.FromArgb(79, 70, 229) : Color.FromArgb(180, 180, 180),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(200, 35),
            Location = new Point(10, 260),
            Cursor = Cursors.Hand,
            Enabled = stock > 0
        };
        addBtn.FlatAppearance.BorderSize = 0;
        var prodId = id;
        addBtn.Click += (s, e) =>
        {
            var dialog = new AddToCartDialog(name, price, stock, size, category);
            if (dialog.ShowDialog() == DialogResult.OK)
                AddToCart(prodId, name, category, dialog.SelectedSize, color, price, imagePath, dialog.SelectedQuantity);
        };

        card.Controls.AddRange([picBox, nameLbl, infoLbl, priceLbl, stockLbl, addBtn]);
        return card;
    }

    private void AddToCart(int productId, string name, string category, string size, string color, decimal price, string? imagePath, int quantity)
    {
        try
        {
            var conn = Database.GetConnection();
            using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = "SELECT Quantity FROM CartItems WHERE CustomerId = @cid AND ProductId = @pid AND Size = @size";
            checkCmd.Parameters.AddWithValue("@cid", customerId);
            checkCmd.Parameters.AddWithValue("@pid", productId);
            checkCmd.Parameters.AddWithValue("@size", size);
            var existing = checkCmd.ExecuteScalar();

            if (existing != null)
            {
                var existingQty = Convert.ToInt32(existing);
                var stockRow = new Func<int>(() =>
                {
                    using var sCmd = conn.CreateCommand();
                    sCmd.CommandText = "SELECT Stock FROM Products WHERE Id = @pid";
                    sCmd.Parameters.AddWithValue("@pid", productId);
                    return Convert.ToInt32(sCmd.ExecuteScalar() ?? 0);
                })();

                var newQty = existingQty + quantity;
                if (newQty > stockRow)
                {
                    MessageBox.Show($"Only {stockRow} available in stock.", "Stock Limit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using var updateCmd = conn.CreateCommand();
                updateCmd.CommandText = "UPDATE CartItems SET Quantity = @qty WHERE CustomerId = @cid AND ProductId = @pid AND Size = @size";
                updateCmd.Parameters.AddWithValue("@qty", newQty);
                updateCmd.Parameters.AddWithValue("@cid", customerId);
                updateCmd.Parameters.AddWithValue("@pid", productId);
                updateCmd.Parameters.AddWithValue("@size", size);
                updateCmd.ExecuteNonQuery();
            }
            else
            {
                using var insertCmd = conn.CreateCommand();
                insertCmd.CommandText = @"INSERT INTO CartItems (CustomerId, ProductId, ProductName, Category, Size, Color, Price, Quantity, ImagePath)
                                         VALUES (@cid, @pid, @name, @cat, @size, @color, @price, @qty, @img)";
                insertCmd.Parameters.AddWithValue("@cid", customerId);
                insertCmd.Parameters.AddWithValue("@pid", productId);
                insertCmd.Parameters.AddWithValue("@name", name);
                insertCmd.Parameters.AddWithValue("@cat", category);
                insertCmd.Parameters.AddWithValue("@size", size);
                insertCmd.Parameters.AddWithValue("@color", color);
                insertCmd.Parameters.AddWithValue("@price", price);
                insertCmd.Parameters.AddWithValue("@qty", quantity);
                insertCmd.Parameters.AddWithValue("@img", (object?)imagePath ?? DBNull.Value);
                insertCmd.ExecuteNonQuery();
            }

            UpdateCartBadge();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error adding to cart:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateCartBadge()
    {
        try
        {
            var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT ISNULL(SUM(Quantity), 0) FROM CartItems WHERE CustomerId = @cid";
            cmd.Parameters.AddWithValue("@cid", customerId);
            var count = cmd.ExecuteScalar()?.ToString() ?? "0";
            cartBadge.Text = count;
            cartBadge.Visible = count != "0";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating cart badge:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OpenCart()
    {
        new CustomerCartForm(customerId, customerName).ShowDialog();
        UpdateCartBadge();
        LoadProducts();
    }

    private void OpenOrders()
    {
        new CustomerOrdersForm(customerId).ShowDialog();
    }

    private void OpenAccount()
    {
        new CustomerAccountForm(customerId).ShowDialog();
    }

    private void Logout()
    {
        this.Close();
        new CustomerLoginForm().Show();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        if (!Application.OpenForms.OfType<CustomerLoginForm>().Any() && !Application.OpenForms.OfType<Form1>().Any())
            Application.Exit();
    }
}
