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
            // SQLite lida melhor com strings para GUIDs na clausula WHERE
            var sql = "SELECT * FROM Orders WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<Order>(sql, new { Id = id.ToString().ToUpper() });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao buscar pedido por Id: {ex.Message}");
            throw; // Repropaga para o AppResult capturar
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
            
            // Garantindo que o ID seja salvo como string em caixa alta para o SQLite
            await connection.ExecuteAsync(sql, new {
                Id = order.Id.ToString().ToUpper(),
                order.CustomerId,
                order.Description,
                Priority = (int)order.Priority,
                Status = (int)order.Status,
                order.CreatedAt,
                order.ProcessedAt,
                order.ProviderId
            });
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
            
            // Forcando a correspondencia com o formato do banco
            await connection.ExecuteAsync(sql, new { 
                Status = (int)order.Status,
                ProcessedAt = order.ProcessedAt,
                ProviderId = order.ProviderId,
                Id = order.Id.ToString().ToUpper()
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao atualizar pedido: {ex.Message}");
            throw;
        }
    }
}
