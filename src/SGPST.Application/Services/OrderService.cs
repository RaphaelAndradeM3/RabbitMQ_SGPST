using SGPST.Application.DTOs;
using SGPST.Application.Interfaces;
using SGPST.Domain.Common;
using SGPST.Domain.Entities;
using SGPST.Domain.Interfaces;

namespace SGPST.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMessageBroker _messageBroker;

    public OrderService(IOrderRepository orderRepository, IMessageBroker messageBroker)
    {
        _orderRepository = orderRepository;
        _messageBroker = messageBroker;
    }

    public async Task<IAppResult<OrderDto>> SubmitOrderAsync(CreateOrderDto createOrderDto)
    {
        try
        {
            // Criando entidade via Factory Method
            var order = Order.Create(
                createOrderDto.CustomerId, 
                createOrderDto.Description, 
                createOrderDto.Priority);

            // Persistindo no banco (Dapper)
            await _orderRepository.AddAsync(order);

            // Publicando no RabbitMQ para processamento assincrono
            await _messageBroker.PublishOrderAsync(order);

            var dto = MapToDto(order);
            return AppResult<OrderDto>.Ok(dto, "Pedido enviado com sucesso para a fila de processamento");
        }
        catch (Exception ex)
        {
            return AppResult<OrderDto>.Failure("Erro ao submeter pedido", ex);
        }
    }

    public async Task<IAppResult<IEnumerable<OrderDto>>> GetPendingOrdersAsync()
    {
        try
        {
            var orders = await _orderRepository.GetPendingOrdersAsync();
            var dtos = orders.Select(MapToDto);
            return AppResult<IEnumerable<OrderDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return AppResult<IEnumerable<OrderDto>>.Failure("Erro ao buscar pedidos pendentes", ex);
        }
    }

    public async Task<IAppResult<IEnumerable<OrderDto>>> GetAllOrdersAsync()
    {
        try
        {
            var orders = await _orderRepository.GetAllAsync();
            var dtos = orders.Select(MapToDto);
            return AppResult<IEnumerable<OrderDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return AppResult<IEnumerable<OrderDto>>.Failure("Erro ao buscar todos os pedidos", ex);
        }
    }

    public async Task<IAppResult> ProcessNextOrderAsync(string providerId)
    {
        try
        {
            // Este metodo sera chamado pelo Worker que consome a fila do RabbitMQ
            // A subscricao real ocorre no Worker, aqui temos a logica de atualizacao do dominio
            
            // Logica simplificada para ser usada pelo consumidor da fila
            return AppResult.Ok("Pronto para processar");
        }
        catch (Exception ex)
        {
            return AppResult.Failure("Erro no fluxo de processamento", ex);
        }
    }

    private OrderDto MapToDto(Order order)
    {
        return new OrderDto(
            order.Id,
            order.CustomerId,
            order.Description,
            order.Priority,
            order.Status,
            order.CreatedAt,
            order.ProcessedAt,
            order.ProviderId
        );
    }
}
