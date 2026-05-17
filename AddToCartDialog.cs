namespace Justine_Apparel_E_Commerce_System;

public class AddToCartDialog : Form
{
    private NumericUpDown qtyBox;
    private ComboBox sizeBox;
    private Button addBtn, cancelBtn;
    private Label productNameLabel;

    public int SelectedQuantity => (int)qtyBox.Value;
    public string SelectedSize => sizeBox.Text;

    public AddToCartDialog(string productName, decimal price, int stock, string currentSize, string category)
    {
        InitializeComponent(productName, price, stock, currentSize, category);
    }

    private void InitializeComponent(string productName, decimal price, int stock, string currentSize, string category)
    {
        this.Text = "";
        this.Size = new Size(520, 440);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(248, 250, 252);
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        var header = new Panel
        {
            Height = 80,
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(79, 70, 229)
        };

        var headerTitle = new Label
        {
            Text = "Add to Cart",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(25, 16)
        };

        var headerSub = new Label
        {
            Text = "Customize your order",
            Font = new Font("Segoe UI", 11, FontStyle.Regular),
            ForeColor = Color.FromArgb(199, 210, 254),
            AutoSize = true,
            Location = new Point(25, 48)
        };

        header.Controls.AddRange([headerTitle, headerSub]);

        var whiteCard = new Panel
        {
            BackColor = Color.White,
            Size = new Size(470, 230),
            Location = new Point(25, 100),
            BorderStyle = BorderStyle.None
        };

        productNameLabel = new Label
        {
            Text = productName,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 30),
            Size = new Size(440, 30),
            Location = new Point(20, 15),
            TextAlign = ContentAlignment.MiddleLeft
        };

        var priceLbl = new Label
        {
            Text = $"₱{price:N2}",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(79, 70, 229),
            Size = new Size(440, 30),
            Location = new Point(20, 48),
            TextAlign = ContentAlignment.MiddleLeft
        };

        var sep = new Label
        {
            Text = "",
            BackColor = Color.FromArgb(230, 232, 236),
            Size = new Size(430, 1),
            Location = new Point(20, 85)
        };

        var qtyLabel = new Label
        {
            Text = "Quantity",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(80, 80, 80),
            AutoSize = true,
            Location = new Point(20, 100)
        };

        qtyBox = new NumericUpDown
        {
            Minimum = 1,
            Maximum = Math.Max(1, stock),
            Value = 1,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Size = new Size(100, 32),
            Location = new Point(20, 125),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(248, 250, 252),
            TextAlign = HorizontalAlignment.Center
        };

        var maxLbl = new Label
        {
            Text = $"max {stock}",
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            ForeColor = Color.FromArgb(150, 150, 150),
            AutoSize = true,
            Location = new Point(125, 133)
        };

        var sizeLabel = new Label
        {
            Text = "Size",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(80, 80, 80),
            AutoSize = true,
            Location = new Point(20, 163)
        };

        sizeBox = new ComboBox
        {
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Size = new Size(430, 34),
            Location = new Point(20, 185),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(248, 250, 252),
            DropDownHeight = 200,
            IntegralHeight = false
        };
        PopulateSizes(category, currentSize);

        whiteCard.Controls.AddRange([productNameLabel, priceLbl, sep, qtyLabel, qtyBox, maxLbl, sizeLabel, sizeBox]);

        addBtn = new Button
        {
            Text = "Add to Cart",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            BackColor = Color.FromArgb(79, 70, 229),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(200, 46),
            Location = new Point(25, 315),
            Cursor = Cursors.Hand
        };
        addBtn.FlatAppearance.BorderSize = 0;
        addBtn.Click += (s, e) =>
        {
            if (sizeBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a size.", "Size Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        };

        cancelBtn = new Button
        {
            Text = "Cancel",
            Font = new Font("Segoe UI", 12, FontStyle.Regular),
            BackColor = Color.FromArgb(230, 232, 236),
            ForeColor = Color.FromArgb(80, 80, 80),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(120, 46),
            Location = new Point(250, 315),
            Cursor = Cursors.Hand,
            DialogResult = DialogResult.Cancel
        };
        cancelBtn.FlatAppearance.BorderSize = 0;

        this.Controls.AddRange([header, whiteCard, addBtn, cancelBtn]);
    }

    private void PopulateSizes(string category, string currentSize)
    {
        var sizes = category switch
        {
            "Tops" => new[] { "XS", "S", "M", "L", "XL", "XXL" },
            "Bottoms" => new[] { "28", "30", "32", "34", "36", "38" },
            "Outerwear" => new[] { "XS", "S", "M", "L", "XL" },
            "Dresses" => new[] { "XS", "S", "M", "L", "XL" },
            "Accessories" => new[] { "One Size" },
            _ => new[] { "XS", "S", "M", "L", "XL" }
        };

        sizeBox.Items.AddRange(sizes);

        if (sizes.Contains(currentSize))
            sizeBox.SelectedItem = currentSize;
        else if (sizeBox.Items.Count > 0)
            sizeBox.SelectedIndex = 0;
    }
}
