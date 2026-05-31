using SGPST.Domain.Entities;

namespace SGPST.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<IEnumerable<Order>> GetAllAsync();
    Task<IEnumerable<Order>> GetPendingOrdersAsync();
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
}
