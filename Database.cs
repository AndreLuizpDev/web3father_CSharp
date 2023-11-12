using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using web3father_CSharp.Models;

public class Database
{
    private string connectionString;

    public Database(string dbPath)
    {
        connectionString = $"Data Source={dbPath};Version=3;";
    }

    public void InitializeDatabase()
    {
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

            // Crie tabelas para produtos e vendedores
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Products (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT,
                        Price REAL
                    );

                    CREATE TABLE IF NOT EXISTS Sellers (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT
                    );
                    CREATE TABLE IF NOT EXISTS Payments (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId BIGINT,
                        PaymentId TEXT,
                        PaymentDate TEXT,
                        DueDate TEXT
                    );
                ";
                command.ExecuteNonQuery();

                // Verifique se as tabelas foram criadas com sucesso
                bool productsTableExists = IsTableExists(connection, "Products");
                bool sellersTableExists = IsTableExists(connection, "Sellers");
                bool paymentsTableExists = IsTableExists(connection, "Payments");

                if (productsTableExists && sellersTableExists && paymentsTableExists)
                {
                    Console.WriteLine("Tabelas foram criadas com sucesso.");
                }
                else
                {
                    Console.WriteLine("Falha ao criar tabelas.");
                }

                // Obtenha o caminho do arquivo do banco de dados
                string databaseFilePath = connection.DataSource;
                Console.WriteLine("Caminho do arquivo do banco de dados: " + databaseFilePath);
            }
        }
    }

    // Outros métodos para consultar e modificar o banco de dados

    // Função para verificar se a tabela existe no banco de dados
    private bool IsTableExists(SQLiteConnection connection, string tableName)
    {
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=@TableName";
            command.Parameters.AddWithValue("@TableName", tableName);
            using (var reader = command.ExecuteReader())
            {
                return reader.HasRows;
            }
        }
    }

    public bool AddProduct(string name, double price)
    {
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO Products (Name, Price) VALUES (@Name, @Price)";
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Price", price);
                int rowsAffected = command.ExecuteNonQuery();

                return rowsAffected > 0; // Retorna true se alguma linha foi afetada (inserção bem-sucedida)
            }
        }
    }
    public bool AddSeller(string name)
    {
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO Sellers (Name) VALUES (@Name)";
                command.Parameters.AddWithValue("@Name", name);
                int rowsAffected = command.ExecuteNonQuery();

                return rowsAffected > 0; // Retorna true se alguma linha foi afetada (inserção bem-sucedida)
            }
        }
    }
    public bool InsertPayment(long userId, string paymentId, string paymentDate, string dueDate)
    {
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO Payments (UserId, PaymentId, PaymentDate, DueDate) VALUES (@UserId, @PaymentId, @PaymentDate, @DueDate)";
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@PaymentId", paymentId);
                command.Parameters.AddWithValue("@PaymentDate", paymentDate);
                command.Parameters.AddWithValue("@DueDate", dueDate);
                int rowsAffected = command.ExecuteNonQuery();

                return rowsAffected > 0; // Retorna true se alguma linha foi afetada (inserção bem-sucedida)
            }
        }
    }

    public List<Product> GetAllProducts()
    {
        List<Product> products = new List<Product>();

        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Id, Name, Price FROM Products";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Product product = new Product
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Price = reader.GetDouble(2)
                        };
                        products.Add(product);
                    }
                }
            }
        }

        return products;
    }
    public List<Seller> GetAllSellers()
    {
        List<Seller> sellers = new List<Seller>();

        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Id, Name FROM Sellers";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Seller seller = new Seller
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        };
                        sellers.Add(seller);
                    }
                }
            }
        }

        return sellers;
    }
    public string GetDueDateByUserId(long userId, string paymentId = null)
    {
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT DueDate FROM Payments WHERE UserId = @UserId OR PaymentId = @PaymentId";
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@PaymentId", paymentId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetString(0); // Retorna a data de vencimento
                    }
                    else
                    {
                        return null; // Se nenhum registro for encontrado
                    }
                }
            }
        }
    }
}
