using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPST.Application.DTOs.User;
using SGPST.Application.Interfaces;

namespace SGPST.Presentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Apenas administradores podem gerenciar usuarios diretamente
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public class ActiveStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _userService.GetByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(new { message = result.Message });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao buscar usuario.", error = ex.Message });
        }
    }

    [HttpPost]
    [AllowAnonymous] // Permitir criacao inicial de usuarios (ex: primeiro admin ou auto-cadastro de cliente)
    public async Task<IActionResult> Create([FromBody] CreateUserDto createUserDto)
    {
        try
        {
            var result = await _userService.CreateAsync(createUserDto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao cadastrar usuario.", error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _userService.GetAllAsync();
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao buscar usuarios.", error = ex.Message });
        }
    }

    public class AssociateClientRequest
    {
        public Guid? ClientId { get; set; }
    }

    [HttpPut("{id}/active")]
    public async Task<IActionResult> SetActive(Guid id, [FromBody] ActiveStatusRequest request)
    {
        try
        {
            var result = await _userService.SetActiveAsync(id, request.IsActive);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao alterar ativacao do usuario.", error = ex.Message });
        }
    }

    [HttpPut("{id}/associate-client")]
    public async Task<IActionResult> AssociateClient(Guid id, [FromBody] AssociateClientRequest request)
    {
        try
        {
            var result = await _userService.AssociateClientAsync(id, request.ClientId);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao associar cliente ao usuario.", error = ex.Message });
        }
    }
}
