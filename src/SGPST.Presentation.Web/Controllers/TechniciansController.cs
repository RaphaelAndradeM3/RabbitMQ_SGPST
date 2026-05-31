using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPST.Application.DTOs.Technician;
using SGPST.Application.Interfaces;

namespace SGPST.Presentation.Web.Controllers;

[Authorize(Roles = "Admin,Atendente")]
public class TechniciansController : Controller
{
    private readonly ITechnicianService _technicianService;
    private readonly IUserService _userService;
    private readonly ILogger<TechniciansController> _logger;

    public TechniciansController(
        ITechnicianService technicianService,
        IUserService userService,
        ILogger<TechniciansController> logger)
    {
        _technicianService = technicianService;
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var result = await _technicianService.GetAllAsync();
            var technicians = result.Data ?? new List<TechnicianDto>();
            return View(technicians);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar tecnicos.");
            return View(new List<TechnicianDto>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        try
        {
            // Carregar todos os usuarios com role "Tecnico"
            var usersResult = await _userService.GetAllAsync();
            var techUsers = (usersResult.Data ?? new List<Application.DTOs.User.UserDto>())
                .Where(u => u.Role.Equals("Tecnico", StringComparison.OrdinalIgnoreCase));

            // Filtrar os que ja sao cadastrados como tecnicos para nao duplicar
            var techsResult = await _technicianService.GetAllAsync();
            var registeredUserIds = (techsResult.Data ?? new List<TechnicianDto>())
                .Select(t => t.UserId)
                .ToHashSet();

            var availableUsers = techUsers.Where(u => !registeredUserIds.Contains(u.Id)).ToList();

            ViewBag.AvailableUsers = availableUsers;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar formulario de novo tecnico.");
            return View();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTechnicianDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var usersResult = await _userService.GetAllAsync();
                ViewBag.AvailableUsers = (usersResult.Data ?? new List<Application.DTOs.User.UserDto>())
                    .Where(u => u.Role.Equals("Tecnico", StringComparison.OrdinalIgnoreCase)).ToList();
                return View(createDto);
            }

            var result = await _technicianService.CreateAsync(createDto);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                var usersResult = await _userService.GetAllAsync();
                ViewBag.AvailableUsers = (usersResult.Data ?? new List<Application.DTOs.User.UserDto>())
                    .Where(u => u.Role.Equals("Tecnico", StringComparison.OrdinalIgnoreCase)).ToList();
                return View(createDto);
            }

            TempData["SuccessMessage"] = "Tecnico cadastrado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar tecnico.");
            ModelState.AddModelError(string.Empty, $"Erro interno: {ex.Message}");
            return View(createDto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAvailability(Guid id, bool isAvailable)
    {
        try
        {
            var result = await _technicianService.SetAvailabilityAsync(id, isAvailable);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = "Disponibilidade do tecnico alterada.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alterar disponibilidade do tecnico {Id}.", id);
            TempData["ErrorMessage"] = $"Erro interno: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}
