namespace Justine_Apparel_E_Commerce_System;

public class Dashboard : Form
{
    private Panel sidebar = null!;
    private Panel headerPanel = null!;
    private Panel contentPanel = null!;
    private Label headerTitle = null!;
    private Label userLabel = null!;
    private string userEmail;
    private string userName;
    private Button btnCustomers = null!, btnProducts = null!, btnOrders = null!, btnPayments = null!, btnExit = null!;

    public Dashboard(string email, string name)
    {
        userEmail = email;
        userName = name;
        InitializeComponent();
        LayoutContent();
        LoadStats();
    }

    private void InitializeComponent()
    {
        this.Text = "Justine Apparel - Dashboard";
        this.WindowState = FormWindowState.Maximized;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(240, 242, 245);

        sidebar = new Panel
        {
            Width = 220,
            Dock = DockStyle.Left,
            BackColor = Color.FromArgb(30, 30, 40)
        };

        var logoLabel = new Label
        {
            Text = "Justine Apparel",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.White,
            Size = new Size(200, 50),
            Location = new Point(10, 15),
            TextAlign = ContentAlignment.MiddleLeft
        };
        sidebar.Controls.Add(logoLabel);

        string[] menuItems = ["Customers", "Products", "Orders", "Payments", "Exit"];
        var btnArray = new Button[5];
        string[] icons = ["👥", "📦", "📋", "💳", "🚪"];
        int yPos = 80;

        for (int i = 0; i < menuItems.Length; i++)
        {
            var idx = i;
            var btn = new Button
            {
                Text = $"  {icons[i]}  {menuItems[i]}",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(200, 200, 210),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Size = new Size(200, 45),
                Location = new Point(10, yPos),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(50, 50, 65);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.Transparent;
            btn.Click += (s, e) => SidebarButtonClick(menuItems[idx]);
            sidebar.Controls.Add(btn);
            btnArray[i] = btn;
            yPos += 50;
        }

        btnCustomers = btnArray[0];
        btnProducts = btnArray[1];
        btnOrders = btnArray[2];
        btnPayments = btnArray[3];
        btnExit = btnArray[4];

        headerPanel = new Panel
        {
            Height = 60,
            Dock = DockStyle.Top,
            BackColor = Color.White
        };

        headerTitle = new Label
        {
            Text = "Dashboard",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 30),
            Location = new Point(20, 12),
            AutoSize = true
        };

        userLabel = new Label
        {
            Text = $"{userEmail}",
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            ForeColor = Color.FromArgb(100, 100, 100),
            Location = new Point(0, 18),
            TextAlign = ContentAlignment.MiddleRight,
            Width = 200,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        headerPanel.Controls.AddRange([headerTitle, userLabel]);

        contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(240, 242, 245),
            AutoScroll = true
        };

        this.Controls.AddRange([contentPanel, headerPanel, sidebar]);
    }

    private void SidebarButtonClick(string item)
    {
        switch (item)
        {
            case "Customers":
                new CustomerForm().ShowDialog();
                break;
            case "Products":
                new ProductForm().ShowDialog();
                break;
            case "Orders":
                new OrderForm().ShowDialog();
                break;
            case "Payments":
                MessageBox.Show("Payments module coming soon.", "Payments", MessageBoxButtons.OK, MessageBoxIcon.Information);
                break;
            case "Exit":
                Application.Exit();
                break;
        }
        LoadStats();
    }

    public void LayoutContent()
    {
        contentPanel.Controls.Clear();

        int cardW = 220;
        int cardH = 120;
        int gap = 20;

        var statsPanel = new Panel
        {
            Location = new Point(20, 20),
            AutoSize = true
        };

        string[] statTitles = ["Total Customers", "Total Products", "Total Orders", "Revenue"];
        string[] statColors = ["#4F46E5", "#10B981", "#F59E0B", "#EF4444"];
        var statValues = new string[4];

        var conn = Database.GetConnection();
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Customers";
            statValues[0] = cmd.ExecuteScalar()?.ToString() ?? "0";

            cmd.CommandText = "SELECT COUNT(*) FROM Products";
            statValues[1] = cmd.ExecuteScalar()?.ToString() ?? "0";

            cmd.CommandText = "SELECT COUNT(*) FROM Orders";
            statValues[2] = cmd.ExecuteScalar()?.ToString() ?? "0";

            cmd.CommandText = "SELECT ISNULL(SUM(Total), 0) FROM Orders";
            statValues[3] = $"₱{Convert.ToDecimal(cmd.ExecuteScalar() ?? 0):N0}";
        }

        for (int i = 0; i < 4; i++)
        {
            var card = new Panel
            {
                Size = new Size(cardW, cardH),
                Location = new Point(i * (cardW + gap), 0),
                BackColor = Color.White
            };

            var dot = new Label
            {
                Text = "●",
                Font = new Font("Segoe UI", 10),
                ForeColor = ColorTranslator.FromHtml(statColors[i]),
                Size = new Size(15, 15),
                Location = new Point(15, 15)
            };

            var title = new Label
            {
                Text = statTitles[i],
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 100, 100),
                Size = new Size(180, 20),
                Location = new Point(35, 13)
            };

            var value = new Label
            {
                Text = statValues[i],
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Size = new Size(190, 40),
                Location = new Point(15, 45)
            };

            card.Controls.AddRange([dot, title, value]);
            statsPanel.Controls.Add(card);
        }

        var quickLabel = new Label
        {
            Text = "Quick Actions",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 30),
            Location = new Point(20, statsPanel.Bottom + 30),
            AutoSize = true
        };

        contentPanel.Controls.Add(statsPanel);
        contentPanel.Controls.Add(quickLabel);

        var actions = new (string, string, Action<string>)[]
        {
            ("Add Customer", "➕", _ => new CustomerForm().ShowDialog()),
            ("Add Product", "📦", _ => new ProductForm().ShowDialog()),
            ("View Orders", "📋", _ => new OrderForm().ShowDialog()),
        };

        int ax = 20;
        int ay = quickLabel.Bottom + 15;
        int btnW = 180;
        int btnH = 50;

        foreach (var (text, icon, action) in actions)
        {
            var btn = new Button
            {
                Text = $"  {icon}  {text}",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(50, 50, 50),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Size = new Size(btnW, btnH),
                Location = new Point(ax, ay),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btn.Click += (s, e) => { action(text); LoadStats(); };
            contentPanel.Controls.Add(btn);
            ax += btnW + 10;
        }
    }

    private void LoadStats()
    {
        if (contentPanel != null)
            LayoutContent();
    }
}
