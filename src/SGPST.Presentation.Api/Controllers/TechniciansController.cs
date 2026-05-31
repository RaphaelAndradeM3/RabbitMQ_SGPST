using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPST.Application.DTOs.Technician;
using SGPST.Application.Interfaces;

namespace SGPST.Presentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requer autenticacao JWT
public class TechniciansController : ControllerBase
{
    private readonly ITechnicianService _technicianService;

    public class LocationRequest
    {
        public string Location { get; set; } = string.Empty;
    }

    public class AvailabilityRequest
    {
        public bool IsAvailable { get; set; }
    }

    public class ActiveStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public TechniciansController(ITechnicianService technicianService)
    {
        _technicianService = technicianService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _technicianService.GetByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(new { message = result.Message });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao buscar tecnico.", error = ex.Message });
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        try
        {
            var result = await _technicianService.GetByUserIdAsync(userId);
            if (!result.Success)
            {
                return NotFound(new { message = result.Message });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao buscar tecnico por UserId.", error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")] // Apenas admin cadastra tecnicos
    public async Task<IActionResult> Create([FromBody] CreateTechnicianDto createDto)
    {
        try
        {
            var result = await _technicianService.CreateAsync(createDto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao cadastrar tecnico.", error = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Atendente")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _technicianService.GetAllAsync();
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao listar tecnicos.", error = ex.Message });
        }
    }

    [HttpGet("available")]
    [Authorize(Roles = "Admin,Atendente")]
    public async Task<IActionResult> GetAvailable()
    {
        try
        {
            var result = await _technicianService.GetAvailableAsync();
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao listar tecnicos disponiveis.", error = ex.Message });
        }
    }

    [HttpPut("{id}/location")]
    [Authorize(Roles = "Admin,Tecnico")]
    public async Task<IActionResult> UpdateLocation(Guid id, [FromBody] LocationRequest request)
    {
        try
        {
            var result = await _technicianService.UpdateLocationAsync(id, request.Location);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao atualizar localizacao.", error = ex.Message });
        }
    }

    [HttpPut("{id}/availability")]
    [Authorize(Roles = "Admin,Tecnico")]
    public async Task<IActionResult> SetAvailability(Guid id, [FromBody] AvailabilityRequest request)
    {
        try
        {
            var result = await _technicianService.SetAvailabilityAsync(id, request.IsAvailable);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao alterar disponibilidade.", error = ex.Message });
        }
    }

    [HttpPut("{id}/active")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetActive(Guid id, [FromBody] ActiveStatusRequest request)
    {
        try
        {
            var result = await _technicianService.SetActiveAsync(id, request.IsActive);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao alterar ativacao do tecnico.", error = ex.Message });
        }
    }
}
