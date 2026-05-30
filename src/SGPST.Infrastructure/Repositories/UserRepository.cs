using Dapper;
using SGPST.Domain.Entities;
using SGPST.Domain.Interfaces;
using SGPST.Infrastructure.Data;

namespace SGPST.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Id = @Id", new { Id = id });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao buscar usuario por Id: {ex.Message}");
            return null;
        }
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Username = @Username", new { Username = username });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao buscar usuario por username: {ex.Message}");
            return null;
        }
    }

    public async Task AddAsync(User user)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = "INSERT INTO Users (Id, Username, Email, IsActive) VALUES (@Id, @Username, @Email, @IsActive)";
            await connection.ExecuteAsync(sql, user);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao adicionar usuario: {ex.Message}");
            throw;
        }
    }
}
