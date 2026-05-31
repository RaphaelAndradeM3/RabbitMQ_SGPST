using System.Data;
using Microsoft.Data.Sqlite;
using Dapper;

namespace SGPST.Infrastructure.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
    void SetupDatabase();
}

public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public override void SetValue(IDbDataParameter parameter, Guid value) => parameter.Value = value.ToString();
    public override Guid Parse(object value) => Guid.Parse((string)value);
}

public class SqliteConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;
    private static bool _typeHandlersRegistered = false;

    public SqliteConnectionFactory(string? connectionString = null)
    {
        if (!_typeHandlersRegistered)
        {
            SqlMapper.AddTypeHandler(new GuidTypeHandler());
            _typeHandlersRegistered = true;
        }

        if (string.IsNullOrEmpty(connectionString))
        {
            // Cria um caminho fixo em AppData para que todos os apps compartilhem o mesmo banco
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbDir = Path.Combine(appData, "SGPST");
            if (!Directory.Exists(dbDir)) Directory.CreateDirectory(dbDir);
            
            var dbPath = Path.Combine(dbDir, "sgpst.db");
            _connectionString = $"Data Source={dbPath}";
        }
        else
        {
            _connectionString = connectionString;
        }
    }

    public IDbConnection CreateConnection()
    {
        try
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            Console.WriteLine($"[DB] Conectado ao banco: {connection.DataSource}");
            return connection;
        }
        catch (Exception ex)
        {
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
