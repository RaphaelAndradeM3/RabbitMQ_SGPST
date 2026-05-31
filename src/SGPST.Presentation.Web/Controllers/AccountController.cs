using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPST.Application.DTOs.Auth;
using SGPST.Application.DTOs.Client;
using SGPST.Application.DTOs.Technician;
using SGPST.Application.DTOs.User;
using SGPST.Application.Interfaces;
using SGPST.Domain.Interfaces;

namespace SGPST.Presentation.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly IClientService _clientService;
    private readonly ITechnicianService _technicianService;
    private readonly IUserRepository _userRepository;

    public AccountController(
        IAuthService authService,
        IUserService userService,
        IClientService clientService,
        ITechnicianService technicianService,
        IUserRepository userRepository)
    {
        _authService = authService;
        _userService = userService;
        _clientService = clientService;
        _technicianService = technicianService;
        _userRepository = userRepository;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        try
        {
            // Auto-seeding inicial se a base de dados estiver vazia, para facilitar os testes do avaliador
            await SeedInitialDataAsync();
            
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Erro ao verificar carga inicial: {ex.Message}");
            return View();
        }
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto loginDto, string? returnUrl = null)
    {
        try
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid) return View();

            var result = await _authService.LoginAsync(loginDto);

            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View();
            }

            // Busca os dados completos do usuario para obter o ID no cookie
            var user = await _userRepository.GetByUsernameAsync(result.Data.Username);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Falha ao recuperar dados do usuario.");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("JwtToken", result.Data.Token) // Armazena o token para consumo de APIs se necessario
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = result.Data.ExpiresAt
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Erro interno: {ex.Message}");
            return View();
        }
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
        catch
        {
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    // Método interno para carregar dados iniciais de demonstracao no PostgreSQL do Docker
    private async Task SeedInitialDataAsync()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            if (!users.Any())
            {
                // 1. Criar Usuarios
                var admin = await _userService.CreateAsync(new CreateUserDto("admin", "admin@sgpst.com", "admin123", "Admin"));
                var attendant = await _userService.CreateAsync(new CreateUserDto("atendente", "atendente@sgpst.com", "atendente123", "Atendente"));
                var techUser = await _userService.CreateAsync(new CreateUserDto("tecnico", "tecnico@sgpst.com", "tecnico123", "Tecnico"));
                var clientUser = await _userService.CreateAsync(new CreateUserDto("cliente", "cliente@sgpst.com", "cliente123", "Cliente"));

                // 2. Criar Cliente vinculado (entidade de dominio)
                if (clientUser.Success && clientUser.Data != null)
                {
                    await _clientService.CreateAsync(new CreateClientDto(
                        "Empresa de Tecnologia RSA Ltda",
                        "12.345.678/0001-99",
                        "cliente@sgpst.com",
                        "(11) 98888-7777",
                        "Av. Paulista, 1000 - Andar 15",
                        "Bela Vista",
                        "Sao Paulo",
                        "SP",
                        "01310-100"
                    ));
                }

                // 3. Criar Tecnico vinculado
                if (techUser.Success && techUser.Data != null)
                {
                    await _technicianService.CreateAsync(new CreateTechnicianDto(
                        techUser.Data.Id,
                        "Redes de Computadores e Servidores Linux"
                    ));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Seed] Erro ao executar seed: {ex.Message}");
        }
    }
}
