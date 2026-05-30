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

    public async Task<IAppResult> UpdateStatusToProcessingAsync(Guid orderId, string providerId)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return AppResult.Failure("Pedido nao encontrado");

            order.StartProcessing(providerId);
            await _orderRepository.UpdateAsync(order);

            return AppResult.Ok("Status atualizado para Em Processamento");
        }
        catch (Exception ex)
        {
            return AppResult.Failure("Erro ao iniciar processamento", ex);
        }
    }

    public async Task<IAppResult> UpdateStatusToCompletedAsync(Guid orderId)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return AppResult.Failure("Pedido nao encontrado");

            order.Complete();
            await _orderRepository.UpdateAsync(order);

            return AppResult.Ok("Status atualizado para Concluido");
        }
        catch (Exception ex)
        {
            return AppResult.Failure("Erro ao finalizar pedido", ex);
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
