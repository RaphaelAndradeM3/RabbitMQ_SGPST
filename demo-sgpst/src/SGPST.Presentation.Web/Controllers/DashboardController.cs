using Microsoft.AspNetCore.Mvc;
using SGPST.Application.Interfaces;

namespace SGPST.Presentation.Web.Controllers;

public class DashboardController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DashboardController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("http://localhost:5042/");
            
            var response = await client.GetAsync("api/Orders");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<SGPST.Domain.Common.AppResult<IEnumerable<SGPST.Application.DTOs.OrderDto>>>();
                return View(result?.Data ?? Enumerable.Empty<SGPST.Application.DTOs.OrderDto>());
            }
            
            return View(Enumerable.Empty<SGPST.Application.DTOs.OrderDto>());
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Erro ao carregar dashboard: {ex.Message}";
            return View(Enumerable.Empty<SGPST.Application.DTOs.OrderDto>());
        }
    }
}
