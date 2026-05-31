using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPST.Application.DTOs.Client;
using SGPST.Application.Interfaces;

namespace SGPST.Presentation.Web.Controllers;

[Authorize(Roles = "Admin,Atendente")]
public class ClientsController : Controller
{
    private readonly IClientService _clientService;
    private readonly ILogger<ClientsController> _logger;

    public ClientsController(IClientService clientService, ILogger<ClientsController> _logger)
    {
        _clientService = clientService;
        this._logger = _logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var result = await _clientService.GetAllAsync();
            var clients = result.Data ?? new List<ClientDto>();
            return View(clients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar clientes.");
            return View(new List<ClientDto>());
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateClientDto createDto)
    {
        try
        {
            if (!ModelState.IsValid) return View(createDto);

            var result = await _clientService.CreateAsync(createDto);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(createDto);
            }

            TempData["SuccessMessage"] = "Cliente cadastrado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar cliente.");
            ModelState.AddModelError(string.Empty, $"Erro interno: {ex.Message}");
            return View(createDto);
        }
    }
}
