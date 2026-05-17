using Microsoft.Data.SqlClient;

namespace Justine_Apparel_E_Commerce_System;

public static class Database
{
    private static readonly string _masterConnString = "Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";
    private static readonly string _dbConnString = "Server=(localdb)\\MSSQLLocalDB;Database=JustineApparel;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";
    private static SqlConnection? _connection;

    public static void Initialize()
    {
        using var masterConn = new SqlConnection(_masterConnString);
        masterConn.Open();
        using var createCmd = masterConn.CreateCommand();
        createCmd.CommandText = "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'JustineApparel') CREATE DATABASE JustineApparel";
        createCmd.ExecuteNonQuery();

        using var cmd = GetConnection().CreateCommand();

        cmd.CommandText = @"
            IF OBJECT_ID(N'Users', N'U') IS NULL
            CREATE TABLE Users (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                Email NVARCHAR(255) NOT NULL UNIQUE,
                Password NVARCHAR(255) NOT NULL,
                Name NVARCHAR(255) NOT NULL,
                Phone NVARCHAR(50),
                Address NVARCHAR(MAX),
                IsAdmin INT DEFAULT 0
            );

            IF OBJECT_ID(N'Products', N'U') IS NULL
            CREATE TABLE Products (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                ProductName NVARCHAR(255) NOT NULL,
                Category NVARCHAR(100) NOT NULL,
                Size NVARCHAR(50),
                Color NVARCHAR(50),
                Price DECIMAL(18,2) NOT NULL,
                Stock INT DEFAULT 0,
                Status NVARCHAR(50) DEFAULT 'Active',
                ImagePath NVARCHAR(MAX)
            );

            IF OBJECT_ID(N'Customers', N'U') IS NULL
            CREATE TABLE Customers (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                Name NVARCHAR(255) NOT NULL,
                Email NVARCHAR(255) NOT NULL UNIQUE,
                Password NVARCHAR(255) NOT NULL DEFAULT 'customer123',
                Phone NVARCHAR(50),
                Address NVARCHAR(MAX),
                Orders INT DEFAULT 0,
                Spent DECIMAL(18,2) DEFAULT 0,
                JoinedDate DATETIME2 DEFAULT GETDATE()
            );

            IF OBJECT_ID(N'Orders', N'U') IS NULL
            CREATE TABLE Orders (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                CustomerId INT NOT NULL,
                CustomerName NVARCHAR(255) NOT NULL,
                OrderDate DATETIME2 DEFAULT GETDATE(),
                Total DECIMAL(18,2) NOT NULL,
                Status NVARCHAR(50) DEFAULT 'Pending',
                FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
            );

            IF OBJECT_ID(N'OrderItems', N'U') IS NULL
            CREATE TABLE OrderItems (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                OrderId INT NOT NULL,
                ProductId INT NOT NULL,
                ProductName NVARCHAR(255) NOT NULL,
                Quantity INT NOT NULL,
                Price DECIMAL(18,2) NOT NULL,
                FOREIGN KEY (OrderId) REFERENCES Orders(Id),
                FOREIGN KEY (ProductId) REFERENCES Products(Id)
            );

            IF OBJECT_ID(N'CartItems', N'U') IS NULL
            CREATE TABLE CartItems (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                CustomerId INT NOT NULL,
                ProductId INT NOT NULL,
                ProductName NVARCHAR(255) NOT NULL,
                Category NVARCHAR(100),
                Size NVARCHAR(50),
                Color NVARCHAR(50),
                Price DECIMAL(18,2) NOT NULL,
                Quantity INT DEFAULT 1,
                ImagePath NVARCHAR(MAX),
                FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
                FOREIGN KEY (ProductId) REFERENCES Products(Id)
            );
        ";
        cmd.ExecuteNonQuery();

        SeedData();
    }

    private static void SeedData()
    {
        using var cmd = _connection!.CreateCommand();

        cmd.CommandText = "IF NOT EXISTS (SELECT 1 FROM Users WHERE IsAdmin = 1) INSERT INTO Users (Email, Password, Name, IsAdmin) VALUES ('admin@gmail.com', 'admin123', 'Admin', 1)";
        cmd.ExecuteNonQuery();

        cmd.CommandText = "IF NOT EXISTS (SELECT 1 FROM Customers WHERE Email = 'customer@gmail.com') INSERT INTO Customers (Name, Email, Password, Phone, Address) VALUES ('Juan Dela Cruz', 'customer@gmail.com', 'customer123', '09123456789', '123 Rizal St., Manila')";
        cmd.ExecuteNonQuery();

        cmd.CommandText = "IF NOT EXISTS (SELECT 1 FROM Products) INSERT INTO Products (ProductName, Category, Size, Color, Price, Stock) VALUES (@n, @c, @s, @co, @p, @st)";
        cmd.Parameters.Clear();

        var products = new[]
        {
            ("Classic White T-Shirt", "Tops", "M", "White", 299.99m, 50),
            ("Black Denim Jacket", "Outerwear", "L", "Black", 1299.99m, 25),
            ("Floral Summer Dress", "Dresses", "S", "Floral", 899.99m, 30),
            ("Slim Fit Chinos", "Bottoms", "32", "Khaki", 699.99m, 40),
            ("Cashmere Scarf", "Accessories", "One Size", "Gray", 499.99m, 15),
            ("Striped Polo Shirt", "Tops", "L", "Navy", 449.99m, 35),
            ("Leather Belt", "Accessories", "34", "Brown", 599.99m, 20),
            ("Denim Shorts", "Bottoms", "M", "Blue", 399.99m, 45),3
        };
        foreach (var (name, cat, size, color, price, stock) in products)
        {
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@n", name);
            cmd.Parameters.AddWithValue("@c", cat);
            cmd.Parameters.AddWithValue("@s", size);
            cmd.Parameters.AddWithValue("@co", color);
            cmd.Parameters.AddWithValue("@p", price);
            cmd.Parameters.AddWithValue("@st", stock);
            cmd.ExecuteNonQuery();
        }
    }

    public static SqlConnection GetConnection()
    {
        if (_connection == null)
        {
            _connection = new SqlConnection(_dbConnString);
            _connection.Open();
        }
        return _connection;
    }
}
