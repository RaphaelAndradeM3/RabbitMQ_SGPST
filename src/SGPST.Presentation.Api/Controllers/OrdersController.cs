using Microsoft.AspNetCore.Mvc;
using SGPST.Application.DTOs;
using SGPST.Application.Interfaces;

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
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
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
            return StatusCode(500, $"Erro interno: {ex.Message}");
        }
    }

    [HttpPatch("{id}/start-processing")]
    public async Task<IActionResult> StartProcessing(Guid id, [FromQuery] string providerId)
    {
        try
        {
            var result = await _orderService.UpdateStatusToProcessingAsync(id, providerId);
            if (result.Success) return Ok(result);
            
            Console.WriteLine($"[API-ERROR] Falha ao iniciar pedido {id}: {result.Message}");
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API-FATAL] Erro ao iniciar pedido {id}: {ex.Message}");
            if (ex.InnerException != null) Console.WriteLine($"[API-FATAL] Inner: {ex.InnerException.Message}");
            return StatusCode(500, $"Erro interno: {ex.Message}");
        }
    }

    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        try
        {
            var result = await _orderService.UpdateStatusToCompletedAsync(id);
            if (result.Success) return Ok(result);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno: {ex.Message}");
        }
    }
}
