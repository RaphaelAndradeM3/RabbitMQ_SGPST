using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPST.Application.DTOs.SupportTicket;
using SGPST.Application.Interfaces;
using SGPST.Domain.Interfaces;

namespace SGPST.Presentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requer autenticacao JWT
public class TicketsController : ControllerBase
{
    private readonly ISupportTicketService _ticketService;

    public class AssignAttendantRequest
    {
        public Guid AttendantId { get; set; }
    }

    public class DisplacementRequest
    {
        public string? Location { get; set; }
    }

    public TicketsController(ISupportTicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _ticketService.GetByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(new { message = result.Message });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao buscar chamado.", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupportTicketDto createDto)
    {
        try
        {
            var result = await _ticketService.CreateAsync(createDto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao abrir chamado tecnico.", error = ex.Message });
        }
    }

    [HttpPut("{id}/assign-attendant")]
    [Authorize(Roles = "Admin,Atendente")]
    public async Task<IActionResult> AssignAttendant(Guid id, [FromBody] AssignAttendantRequest request)
    {
        try
        {
            var result = await _ticketService.AssignAttendantAsync(id, request.AttendantId);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao atribuir atendente.", error = ex.Message });
        }
    }

    [HttpPut("assign-technician")]
    [Authorize(Roles = "Admin,Atendente")]
    public async Task<IActionResult> AssignTechnician([FromBody] AssignTechnicianDto assignDto)
    {
        try
        {
            var result = await _ticketService.AssignTechnicianAsync(assignDto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao designar tecnico.", error = ex.Message });
        }
    }

    [HttpPut("{id}/start-displacement")]
    [Authorize(Roles = "Admin,Tecnico")]
    public async Task<IActionResult> StartDisplacement(Guid id, [FromBody] DisplacementRequest request)
    {
        try
        {
            var result = await _ticketService.StartDisplacementAsync(id, request.Location);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao iniciar deslocamento.", error = ex.Message });
        }
    }

    [HttpPut("{id}/end-displacement")]
    [Authorize(Roles = "Admin,Tecnico")]
    public async Task<IActionResult> EndDisplacement(Guid id, [FromBody] DisplacementRequest request)
    {
        try
        {
            var result = await _ticketService.EndDisplacementAsync(id, request.Location);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao finalizar deslocamento.", error = ex.Message });
        }
    }

    [HttpPut("complete")]
    [Authorize(Roles = "Admin,Atendente,Tecnico")]
    public async Task<IActionResult> Complete([FromBody] CompleteTicketDto completeDto)
    {
        try
        {
            var result = await _ticketService.CompleteTicketAsync(completeDto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao finalizar chamado.", error = ex.Message });
        }
    }

    [HttpPut("cancel")]
    public async Task<IActionResult> Cancel([FromBody] CancelTicketDto cancelDto)
    {
        try
        {
            var result = await _ticketService.CancelTicketAsync(cancelDto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao cancelar chamado.", error = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Atendente")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _ticketService.GetAllAsync();
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao listar todos os chamados.", error = ex.Message });
        }
    }

    [HttpGet("status/{status}")]
    [Authorize(Roles = "Admin,Atendente")]
    public async Task<IActionResult> GetByStatus(int status)
    {
        try
        {
            var result = await _ticketService.GetByStatusAsync(status);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao listar chamados por status.", error = ex.Message });
        }
    }

    [HttpGet("client/{clientId}")]
    public async Task<IActionResult> GetByClient(Guid clientId)
    {
        try
        {
            var result = await _ticketService.GetByClientIdAsync(clientId);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao buscar chamados do cliente.", error = ex.Message });
        }
    }

    [HttpGet("technician/{technicianId}")]
    [Authorize(Roles = "Admin,Atendente,Tecnico")]
    public async Task<IActionResult> GetByTechnician(Guid technicianId)
    {
        try
        {
            var result = await _ticketService.GetByTechnicianIdAsync(technicianId);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao buscar chamados do tecnico.", error = ex.Message });
        }
    }

    [HttpGet("{id}/displacements")]
    public async Task<IActionResult> GetDisplacements(Guid id, [FromServices] IDisplacementLogRepository repository)
    {
        try
        {
            var logs = await repository.GetByTicketIdAsync(id);
            var result = logs.Select(l => new {
                l.Id,
                l.TicketId,
                l.DepartureTime,
                l.ArrivalTime,
                l.StartLocation,
                l.EndLocation
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao buscar logs de deslocamento.", error = ex.Message });
        }
    }
}
