using SGPST.Application.DTOs;
using SGPST.Domain.Interfaces;

namespace SGPST.Application.Interfaces;

public interface IOrderService : IBaseService
{
    Task<IAppResult<OrderDto>> SubmitOrderAsync(CreateOrderDto createOrderDto);
    Task<IAppResult<IEnumerable<OrderDto>>> GetPendingOrdersAsync();
    Task<IAppResult<IEnumerable<OrderDto>>> GetAllOrdersAsync();
    Task<IAppResult> ProcessNextOrderAsync(string providerId);
}
