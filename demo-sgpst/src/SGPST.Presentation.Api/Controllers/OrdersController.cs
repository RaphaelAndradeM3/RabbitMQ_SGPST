using Serilog;
using Microsoft.AspNetCore.Mvc;
using SGPST.Application.DTOs;
using SGPST.Application.Interfaces;
using SGPST.Infrastructure.Data;

namespace SGPST.Presentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrderDto)
    {
        try
        {
            var result = await _orderService.SubmitOrderAsync(createOrderDto);
            if (result.Success)
            {
                Log.Information("Pedido criado: {OrderId}", result.Data?.Id);
                return Ok(result);
            }
            Log.Warning("Falha ao criar pedido: {Message}", result.Message);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro critico em CreateOrder");
            return StatusCode(500, $"Erro interno: {ex.Message}");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _orderService.GetAllOrdersAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro critico em GetAll");
            return StatusCode(500, $"Erro interno: {ex.Message}");
        }
    }

    [HttpPatch("{id}/start-processing")]
    public async Task<IActionResult> StartProcessing(Guid id, [FromQuery] string providerId)
    {
        try
        {
            Log.Information("Solicitacao start-processing para pedido {OrderId} por {ProviderId}", id, providerId);
            var result = await _orderService.UpdateStatusToProcessingAsync(id, providerId);
            if (result.Success) return Ok(result);
            
            Log.Warning("Falha ao iniciar pedido {OrderId}: {Message}", id, result.Message);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro fatal ao iniciar pedido {OrderId}", id);
            return StatusCode(500, $"Erro interno: {ex.Message}");
        }
    }

    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        try
        {
            Log.Information("Solicitacao complete para pedido {OrderId}", id);
            var result = await _orderService.UpdateStatusToCompletedAsync(id);
            if (result.Success) return Ok(result);
            
            Log.Warning("Falha ao completar pedido {OrderId}: {Message}", id, result.Message);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro fatal ao completar pedido {OrderId}", id);
            return StatusCode(500, $"Erro interno: {ex.Message}");
        }
    }
}
