using Microsoft.AspNetCore.Mvc;
using SGPST.Application.Interfaces;

namespace SGPST.Presentation.Web.Controllers;

public class DashboardController : Controller
{
    private readonly IOrderService _orderService;

    public DashboardController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var result = await _orderService.GetAllOrdersAsync();
            if (result.Success)
            {
                return View(result.Data);
            }
            return View(Enumerable.Empty<SGPST.Application.DTOs.OrderDto>());
        }
        catch (Exception ex)
        {
            // Logica simples de erro para o prototipo
            ViewBag.Error = $"Erro ao carregar dashboard: {ex.Message}";
            return View(Enumerable.Empty<SGPST.Application.DTOs.OrderDto>());
        }
    }
}
