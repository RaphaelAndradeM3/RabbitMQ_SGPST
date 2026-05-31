using Serilog;
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
            Log.Debug("[OrderService] Buscando pedido {OrderId} no banco para iniciar processamento...", orderId);
            var order = await _orderRepository.GetByIdAsync(orderId);
            
            if (order == null) 
            {
                Log.Warning("[OrderService] Pedido {OrderId} NAO ENCONTRADO no banco durante a tentativa de iniciar processamento.", orderId);
                return AppResult.Failure($"Pedido {orderId} nao encontrado no banco de dados.");
            }

            Log.Information("[OrderService] Pedido {OrderId} encontrado. Alterando status para EmProcessamento por {ProviderId}", orderId, providerId);
            order.StartProcessing(providerId);
            await _orderRepository.UpdateAsync(order);

            return AppResult.Ok("Status atualizado para Em Processamento");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[OrderService] Erro critico ao tentar iniciar processamento do pedido {OrderId}", orderId);
            return AppResult.Failure("Erro interno ao iniciar processamento", ex);
        }
    }

    public async Task<IAppResult> UpdateStatusToCompletedAsync(Guid orderId)
    {
        try
        {
            Log.Debug("[OrderService] Buscando pedido {OrderId} no banco para finalizar...", orderId);
            var order = await _orderRepository.GetByIdAsync(orderId);
            
            if (order == null) 
            {
                Log.Warning("[OrderService] Pedido {OrderId} NAO ENCONTRADO no banco durante a tentativa de finalizacao.", orderId);
                return AppResult.Failure("Pedido nao encontrado");
            }

            order.Complete();
            await _orderRepository.UpdateAsync(order);

            Log.Information("[OrderService] Pedido {OrderId} finalizado com sucesso.", orderId);
            return AppResult.Ok("Status atualizado para Concluido");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[OrderService] Erro critico ao finalizar pedido {OrderId}", orderId);
            return AppResult.Failure("Erro interno ao finalizar pedido", ex);
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
