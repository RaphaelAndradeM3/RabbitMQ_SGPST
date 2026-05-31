using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPST.Application.DTOs.ServicePrice;
using SGPST.Application.Interfaces;

namespace SGPST.Presentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requer autenticacao JWT
public class ServicePricesController : ControllerBase
{
    private readonly IServicePriceService _servicePriceService;

    public class ActiveStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public ServicePricesController(IServicePriceService servicePriceService)
    {
        _servicePriceService = servicePriceService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _servicePriceService.GetByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(new { message = result.Message });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao buscar item do catalogo de precos.", error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")] // Apenas administradores alteram a tabela de precos base
    public async Task<IActionResult> Create([FromBody] CreateServicePriceDto createDto)
    {
        try
        {
            var result = await _servicePriceService.CreateAsync(createDto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao cadastrar servico no catalogo.", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateServicePriceDto updateDto)
    {
        try
        {
            var result = await _servicePriceService.UpdateAsync(id, updateDto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao atualizar item do catalogo.", error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _servicePriceService.GetAllAsync();
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao listar catalogo de precos.", error = ex.Message });
        }
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        try
        {
            var result = await _servicePriceService.GetActiveAsync();
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao listar itens ativos do catalogo.", error = ex.Message });
        }
    }

    [HttpPut("{id}/active")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetActive(Guid id, [FromBody] ActiveStatusRequest request)
    {
        try
        {
            var result = await _servicePriceService.SetActiveAsync(id, request.IsActive);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao alterar ativacao do item do catalogo.", error = ex.Message });
        }
    }
}
