using Microsoft.AspNetCore.Mvc;
using SGPST.Application.DTOs.Auth;
using SGPST.Application.Interfaces;

namespace SGPST.Presentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // Rota publica para realizar autenticacao e receber o Token JWT
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var result = await _authService.LoginAsync(loginDto);
            
            if (!result.Success)
            {
                return Unauthorized(new { message = result.Message });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno ao realizar autenticacao.", error = ex.Message });
        }
    }
}
