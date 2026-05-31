using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPST.Application.DTOs.Client;
using SGPST.Application.Interfaces;

namespace SGPST.Presentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requer autenticacao JWT
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public class ActiveStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _clientService.GetByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(new { message = result.Message });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao buscar cliente.", error = ex.Message });
        }
    }

    [HttpGet("document/{document}")]
    [Authorize(Roles = "Admin,Atendente")]
    public async Task<IActionResult> GetByDocument(string document)
    {
        try
        {
            var result = await _clientService.GetByDocumentAsync(document);
            if (!result.Success)
            {
                return NotFound(new { message = result.Message });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao buscar cliente por documento.", error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Atendente")] // Apenas admin e atendente cadastram clientes diretamente
    public async Task<IActionResult> Create([FromBody] CreateClientDto createDto)
    {
        try
        {
            var result = await _clientService.CreateAsync(createDto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao cadastrar cliente.", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Atendente")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateClientDto updateDto)
    {
        try
        {
            var result = await _clientService.UpdateAsync(id, updateDto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao atualizar cliente.", error = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Atendente")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _clientService.GetAllAsync();
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao listar clientes.", error = ex.Message });
        }
    }

    [HttpPut("{id}/active")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetActive(Guid id, [FromBody] ActiveStatusRequest request)
    {
        try
        {
            var result = await _clientService.SetActiveAsync(id, request.IsActive);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao alterar ativacao do cliente.", error = ex.Message });
        }
    }
}
