using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPST.Application.DTOs.ServicePrice;
using SGPST.Application.Interfaces;

namespace SGPST.Presentation.Web.Controllers;

[Authorize]
public class ServicePricesController : Controller
{
    private readonly IServicePriceService _servicePriceService;
    private readonly ILogger<ServicePricesController> _logger;

    public ServicePricesController(IServicePriceService servicePriceService, ILogger<ServicePricesController> logger)
    {
        _servicePriceService = servicePriceService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var result = await _servicePriceService.GetAllAsync();
            var servicePrices = result.Data ?? new List<ServicePriceDto>();
            return View(servicePrices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar catalogo de precos.");
            return View(new List<ServicePriceDto>());
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Atendente")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Atendente")]
    public async Task<IActionResult> Create(CreateServicePriceDto createDto)
    {
        try
        {
            if (!ModelState.IsValid) return View(createDto);

            var result = await _servicePriceService.CreateAsync(createDto);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(createDto);
            }

            TempData["SuccessMessage"] = "Servico adicionado ao catalogo com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar servico no catalogo.");
            ModelState.AddModelError(string.Empty, $"Erro interno: {ex.Message}");
            return View(createDto);
        }
    }
}
