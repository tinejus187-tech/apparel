using Justine_Apparel_E_Commerce_System.Models;

namespace Justine_Apparel_E_Commerce_System;

public class ProductForm : Form
{
    private TextBox searchBox;
    private DataGridView grid;
    private Button addBtn, editBtn, deleteBtn, refreshBtn, stockBtn;
    private PictureBox previewBox;

    public ProductForm()
    {
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        this.Text = "Product Management";
        this.Size = new Size(1100, 650);
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
            Text = "Products",
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

        topPanel.Controls.AddRange([title, searchBox]);

        addBtn = CreateToolButton("Add Product", Color.FromArgb(79, 70, 229), 15, 70);
        addBtn.Click += (s, e) => AddProduct();

        editBtn = CreateToolButton("Edit Product", Color.FromArgb(16, 185, 129), 140, 70);
        editBtn.Click += (s, e) => EditProduct();

        deleteBtn = CreateToolButton("Delete", Color.FromArgb(239, 68, 68), 265, 70);
        deleteBtn.Click += (s, e) => DeleteProduct();

        refreshBtn = CreateToolButton("Refresh", Color.FromArgb(100, 100, 100), 390, 70);
        refreshBtn.Click += (s, e) => LoadData();

        stockBtn = CreateToolButton("Update Stock", Color.FromArgb(245, 158, 11), 515, 70);
        stockBtn.Click += (s, e) => UpdateStock();

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
        grid.SelectionChanged += Grid_SelectionChanged;

        previewBox = new PictureBox
        {
            Size = new Size(150, 150),
            BackColor = Color.FromArgb(230, 230, 235),
            BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.Zoom,
            Visible = false
        };

        var bottomPanel = new Panel
        {
            Height = 170,
            Dock = DockStyle.Bottom,
            BackColor = Color.FromArgb(240, 242, 245)
        };

        var previewLabel = new Label
        {
            Text = "Product Image Preview:",
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            ForeColor = Color.FromArgb(100, 100, 100),
            Location = new Point(15, 10),
            AutoSize = true
        };

        previewBox.Location = new Point(15, 35);
        bottomPanel.Controls.AddRange([previewLabel, previewBox]);

        this.Controls.AddRange([grid, bottomPanel, deleteBtn, editBtn, addBtn, refreshBtn, stockBtn, topPanel]);
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
            cmd.CommandText = "SELECT Id, ProductName, Category, Size, Color, Price, Stock, Status, ImagePath FROM Products ORDER BY ProductName";
        else
        {
            cmd.CommandText = "SELECT Id, ProductName, Category, Size, Color, Price, Stock, Status, ImagePath FROM Products WHERE ProductName LIKE @search OR Category LIKE @search ORDER BY ProductName";
            cmd.Parameters.AddWithValue("@search", $"%{search}%");
        }

        var dt = new System.Data.DataTable();
        using var reader = cmd.ExecuteReader();
        dt.Load(reader);
        grid.DataSource = dt;

        if (grid.Columns.Contains("Id"))
            grid.Columns["Id"].Visible = false;
        if (grid.Columns.Contains("ImagePath"))
            grid.Columns["ImagePath"].Visible = false;
    }

    private void Grid_SelectionChanged(object? sender, EventArgs e)
    {
        if (grid.CurrentRow != null && grid.CurrentRow.Cells["ImagePath"].Value != null)
        {
            var path = grid.CurrentRow.Cells["ImagePath"].Value?.ToString();
            if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
            {
                previewBox.Image = Image.FromFile(path);
                previewBox.Visible = true;
                return;
            }
        }
        previewBox.Visible = false;
    }

    private void AddProduct()
    {
        var detail = new ProductDetailForm();
        if (detail.ShowDialog() == DialogResult.OK)
        {
            var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Products (ProductName, Category, Size, Color, Price, Stock, Status, ImagePath) VALUES (@name, @cat, @size, @color, @price, @stock, @status, @img)";
            cmd.Parameters.AddWithValue("@name", detail.ProductItemName);
            cmd.Parameters.AddWithValue("@cat", detail.ProductCategory);
            cmd.Parameters.AddWithValue("@size", detail.ProductSize);
            cmd.Parameters.AddWithValue("@color", detail.ProductColor);
            cmd.Parameters.AddWithValue("@price", detail.ProductPrice);
            cmd.Parameters.AddWithValue("@stock", detail.ProductStock);
            cmd.Parameters.AddWithValue("@status", detail.ProductStatus);
            cmd.Parameters.AddWithValue("@img", (object?)detail.ProductImagePath ?? DBNull.Value);
            cmd.ExecuteNonQuery();
            LoadData();
        }
    }

    private void EditProduct()
    {
        if (grid.CurrentRow == null) return;
        var id = (int)(grid.CurrentRow.Cells["Id"].Value ?? 0);

        var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Products WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return;

        var detail = new ProductDetailForm
        {
            ProductItemName = reader.GetString(1),
            ProductCategory = reader.GetString(2),
            ProductSize = reader.IsDBNull(3) ? "" : reader.GetString(3),
            ProductColor = reader.IsDBNull(4) ? "" : reader.GetString(4),
            ProductPrice = reader.GetDecimal(5),
            ProductStock = reader.GetInt32(6),
            ProductStatus = reader.GetString(7),
            ProductImagePath = reader.IsDBNull(8) ? null : reader.GetString(8)
        };

        if (detail.ShowDialog() == DialogResult.OK)
        {
            var conn2 = Database.GetConnection();
            using var cmd2 = conn2.CreateCommand();
            cmd2.CommandText = "UPDATE Products SET ProductName = @name, Category = @cat, Size = @size, Color = @color, Price = @price, Stock = @stock, Status = @status, ImagePath = @img WHERE Id = @id";
            cmd2.Parameters.AddWithValue("@id", id);
            cmd2.Parameters.AddWithValue("@name", detail.ProductItemName);
            cmd2.Parameters.AddWithValue("@cat", detail.ProductCategory);
            cmd2.Parameters.AddWithValue("@size", detail.ProductSize);
            cmd2.Parameters.AddWithValue("@color", detail.ProductColor);
            cmd2.Parameters.AddWithValue("@price", detail.ProductPrice);
            cmd2.Parameters.AddWithValue("@stock", detail.ProductStock);
            cmd2.Parameters.AddWithValue("@status", detail.ProductStatus);
            cmd2.Parameters.AddWithValue("@img", (object?)detail.ProductImagePath ?? DBNull.Value);
            cmd2.ExecuteNonQuery();
            LoadData();
        }
    }

    private void DeleteProduct()
    {
        if (grid.CurrentRow == null) return;
        var id = (int)(grid.CurrentRow.Cells["Id"].Value ?? 0);
        var name = grid.CurrentRow.Cells["ProductName"].Value?.ToString() ?? "";

        if (MessageBox.Show($"Delete product '{name}'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
        {
            var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Products WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            LoadData();
        }
    }

    private void UpdateStock()
    {
        if (grid.CurrentRow == null) return;
        var id = (int)(grid.CurrentRow.Cells["Id"].Value ?? 0);
        var name = grid.CurrentRow.Cells["ProductName"].Value?.ToString() ?? "";
        var currentStock = grid.CurrentRow.Cells["Stock"].Value?.ToString() ?? "0";

        var input = Microsoft.VisualBasic.Interaction.InputBox(
            $"Update stock for \"{name}\":\nEnter new quantity:",
            "Update Stock",
            currentStock);

        if (string.IsNullOrEmpty(input)) return;

        if (!int.TryParse(input.Trim(), out var newStock) || newStock < 0)
        {
            MessageBox.Show("Please enter a valid non-negative number.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var conn = Database.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Products SET Stock = @stock WHERE Id = @id";
        cmd.Parameters.AddWithValue("@stock", newStock);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
        LoadData();
    }
}

public class ProductDetailForm : Form
{
    private TextBox nameBox, sizeBox, colorBox, priceBox, stockBox, imagePathBox;
    private ComboBox categoryBox, statusBox;
    private Button saveBtn, cancelBtn, browseBtn;
    private PictureBox imagePreview;

    public string ProductItemName { get => nameBox.Text.Trim(); set => nameBox.Text = value; }
    public string ProductCategory { get => categoryBox.SelectedItem?.ToString() ?? ""; set => categoryBox.SelectedItem = value; }
    public string ProductSize { get => sizeBox.Text.Trim(); set => sizeBox.Text = value; }
    public string ProductColor { get => colorBox.Text.Trim(); set => colorBox.Text = value; }
    public decimal ProductPrice { get => decimal.TryParse(priceBox.Text.Trim(), out var p) ? p : 0; set => priceBox.Text = value.ToString("F2"); }
    public int ProductStock { get => int.TryParse(stockBox.Text.Trim(), out var s) ? s : 0; set => stockBox.Text = value.ToString(); }
    public string ProductStatus { get => statusBox.SelectedItem?.ToString() ?? "Active"; set => statusBox.SelectedItem = value; }
    public string? ProductImagePath { get => imagePathBox.Text.Trim(); set => imagePathBox.Text = value ?? ""; }

    public ProductDetailForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Product Details";
        this.Size = new Size(500, 520);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.White;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        int y = 15;
        (nameBox, y) = AddField("Product Name", y);

        var catLbl = new Label { Text = "Category", Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(100, 100, 100), Size = new Size(220, 20), Location = new Point(25, y) };
        categoryBox = new ComboBox
        {
            Font = new Font("Segoe UI", 11),
            Size = new Size(220, 25),
            Location = new Point(25, y + 22),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        categoryBox.Items.AddRange(["Tops", "Bottoms", "Dresses", "Outerwear", "Accessories"]);
        categoryBox.SelectedIndex = 0;
        this.Controls.AddRange([catLbl, categoryBox]);
        y += 60;

        (sizeBox, y) = AddField("Size", y);
        (colorBox, y) = AddField("Color", y);
        (priceBox, y) = AddField("Price", y);
        (stockBox, y) = AddField("Stock", y);

        var statusLbl = new Label { Text = "Status", Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(100, 100, 100), Size = new Size(220, 20), Location = new Point(25, y) };
        statusBox = new ComboBox
        {
            Font = new Font("Segoe UI", 11),
            Size = new Size(220, 25),
            Location = new Point(25, y + 22),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        statusBox.Items.AddRange(["Active", "Inactive"]);
        statusBox.SelectedIndex = 0;
        this.Controls.AddRange([statusLbl, statusBox]);
        y += 60;

        var imgLbl = new Label { Text = "Image File", Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(100, 100, 100), Size = new Size(220, 20), Location = new Point(25, y) };
        imagePathBox = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Size = new Size(280, 25),
            Location = new Point(25, y + 22),
            BorderStyle = BorderStyle.FixedSingle,
            ReadOnly = true
        };
        browseBtn = new Button
        {
            Text = "Browse",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(50, 50, 50),
            BackColor = Color.FromArgb(230, 230, 235),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(80, 27),
            Location = new Point(310, y + 21),
            Cursor = Cursors.Hand
        };
        browseBtn.FlatAppearance.BorderSize = 1;
        browseBtn.Click += BrowseBtn_Click;

        imagePreview = new PictureBox
        {
            Size = new Size(100, 100),
            Location = new Point(310, y - 30),
            BackColor = Color.FromArgb(240, 242, 245),
            BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.Zoom
        };

        this.Controls.AddRange([imgLbl, imagePathBox, browseBtn, imagePreview]);
        y += 60;

        saveBtn = new Button
        {
            Text = "Save",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            BackColor = Color.FromArgb(79, 70, 229),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(120, 35),
            Location = new Point(120, y),
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
            Location = new Point(260, y),
            Cursor = Cursors.Hand,
            DialogResult = DialogResult.Cancel
        };
        cancelBtn.FlatAppearance.BorderSize = 0;

        this.Controls.AddRange([saveBtn, cancelBtn]);
    }

    private (TextBox, int) AddField(string label, int y)
    {
        var lbl = new Label
        {
            Text = label,
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            ForeColor = Color.FromArgb(100, 100, 100),
            Size = new Size(220, 20),
            Location = new Point(25, y)
        };
        var box = new TextBox
        {
            Font = new Font("Segoe UI", 11),
            Size = new Size(220, 25),
            Location = new Point(25, y + 22),
            BorderStyle = BorderStyle.FixedSingle
        };
        this.Controls.AddRange([lbl, box]);
        return (box, y + 60);
    }

    private void BrowseBtn_Click(object? sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp",
            Title = "Select Product Image"
        };
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            imagePathBox.Text = dlg.FileName;
            imagePreview.Image = Image.FromFile(dlg.FileName);
            imagePreview.Visible = true;
        }
    }
}
