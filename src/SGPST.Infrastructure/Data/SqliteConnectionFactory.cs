using System.Data;
using Microsoft.Data.Sqlite;

namespace SGPST.Infrastructure.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
    void SetupDatabase();
}

public class SqliteConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string connectionString = "Data Source=sgpst.db")
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        try
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }
        catch (Exception ex)
        {
            // Padrao de log simples para o prototipo
            Console.WriteLine($"Erro ao abrir conexao com o banco: {ex.Message}");
            throw;
        }
    }

    public void SetupDatabase()
    {
        try
        {
            using var connection = CreateConnection();
            var command = connection.CreateCommand();
            
            // Tabela de Pedidos
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Orders (
                    Id GUID PRIMARY KEY,
                    CustomerId TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    Priority INTEGER NOT NULL,
                    Status INTEGER NOT NULL,
                    CreatedAt DATETIME NOT NULL,
                    ProcessedAt DATETIME NULL,
                    ProviderId TEXT NULL
                );
                
                CREATE TABLE IF NOT EXISTS Users (
                    Id GUID PRIMARY KEY,
                    Username TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    IsActive INTEGER NOT NULL
                );
            ";
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao configurar o banco de dados: {ex.Message}");
        }
    }
}
