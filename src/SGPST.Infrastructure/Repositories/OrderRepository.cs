using Dapper;
using SGPST.Domain.Entities;
using SGPST.Domain.Interfaces;
using SGPST.Infrastructure.Data;

namespace SGPST.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public OrderRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Order>(
                "SELECT * FROM Orders WHERE Id = @Id", new { Id = id.ToString().ToUpper() });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao buscar pedido por Id: {ex.Message}");
            return null;
        }
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<Order>("SELECT * FROM Orders");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao buscar todos os pedidos: {ex.Message}");
            return Enumerable.Empty<Order>();
        }
    }

    public async Task<IEnumerable<Order>> GetPendingOrdersAsync()
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<Order>(
                "SELECT * FROM Orders WHERE Status = @Status", 
                new { Status = (int)OrderStatus.Pendente });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao buscar pedidos pendentes: {ex.Message}");
            return Enumerable.Empty<Order>();
        }
    }

    public async Task AddAsync(Order order)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                INSERT INTO Orders (Id, CustomerId, Description, Priority, Status, CreatedAt, ProcessedAt, ProviderId)
                VALUES (@Id, @CustomerId, @Description, @Priority, @Status, @CreatedAt, @ProcessedAt, @ProviderId)";
            
            await connection.ExecuteAsync(sql, order);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao adicionar pedido: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateAsync(Order order)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                UPDATE Orders 
                SET Status = @Status, 
                    ProcessedAt = @ProcessedAt, 
                    ProviderId = @ProviderId 
                WHERE Id = @Id";
            
            await connection.ExecuteAsync(sql, order);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao atualizar pedido: {ex.Message}");
            throw;
        }
    }
}
