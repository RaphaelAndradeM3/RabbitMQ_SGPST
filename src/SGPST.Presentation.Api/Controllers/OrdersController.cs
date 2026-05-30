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
    private const string AppName = "API";

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
                FileLogger.Log(AppName, $"Pedido criado: {result.Data?.Id}");
                return Ok(result);
            }
            FileLogger.Log(AppName, $"Falha ao criar pedido: {result.Message}");
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            FileLogger.LogError(AppName, "Erro critico em CreateOrder", ex);
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
            FileLogger.LogError(AppName, "Erro critico em GetAll", ex);
            return StatusCode(500, $"Erro interno: {ex.Message}");
        }
    }

    [HttpPatch("{id}/start-processing")]
    public async Task<IActionResult> StartProcessing(Guid id, [FromQuery] string providerId)
    {
        try
        {
            FileLogger.Log(AppName, $"Solicitacao start-processing para pedido {id} por {providerId}");
            var result = await _orderService.UpdateStatusToProcessingAsync(id, providerId);
            if (result.Success) return Ok(result);
            
            FileLogger.Log(AppName, $"Falha ao iniciar pedido {id}: {result.Message}");
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            FileLogger.LogError(AppName, $"Erro fatal ao iniciar pedido {id}", ex);
            return StatusCode(500, $"Erro interno: {ex.Message}");
        }
    }

    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        try
        {
            FileLogger.Log(AppName, $"Solicitacao complete para pedido {id}");
            var result = await _orderService.UpdateStatusToCompletedAsync(id);
            if (result.Success) return Ok(result);
            
            FileLogger.Log(AppName, $"Falha ao completar pedido {id}: {result.Message}");
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            FileLogger.LogError(AppName, $"Erro fatal ao completar pedido {id}", ex);
            return StatusCode(500, $"Erro interno: {ex.Message}");
        }
    }
}
