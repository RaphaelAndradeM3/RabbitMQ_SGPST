using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPST.Application.Interfaces;
using SGPST.Presentation.Web.Models;

namespace SGPST.Presentation.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ISupportTicketService _supportTicketService;
    private readonly IClientService _clientService;
    private readonly ITechnicianService _technicianService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        ISupportTicketService supportTicketService,
        IClientService clientService,
        ITechnicianService technicianService,
        ILogger<HomeController> logger)
    {
        _supportTicketService = supportTicketService;
        _clientService = clientService;
        _technicianService = technicianService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? string.Empty;
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty;
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            
            var viewModel = new DashboardViewModel();
            IEnumerable<Application.DTOs.SupportTicket.SupportTicketDto> tickets = new List<Application.DTOs.SupportTicket.SupportTicketDto>();

            // 1. Recuperar Chamados com base no perfil de acesso (Role)
            if (role == "Admin" || role == "Atendente")
            {
                var ticketsResult = await _supportTicketService.GetAllAsync();
                if (ticketsResult.Success && ticketsResult.Data != null)
                {
                    tickets = ticketsResult.Data;
                }

                // Admin e Atendentes veem totais de clientes e tecnicos
                var clientsResult = await _clientService.GetAllAsync();
                if (clientsResult.Success && clientsResult.Data != null)
                {
                    viewModel.TotalClients = clientsResult.Data.Count();
                }

                var techsResult = await _technicianService.GetAllAsync();
                if (techsResult.Success && techsResult.Data != null)
                {
                    viewModel.TotalTechnicians = techsResult.Data.Count();
                }
            }
            else if (role == "Tecnico")
            {
                if (Guid.TryParse(userIdStr, out var userId))
                {
                    var techResult = await _technicianService.GetByUserIdAsync(userId);
                    if (techResult.Success && techResult.Data != null)
                    {
                        var ticketsResult = await _supportTicketService.GetByTechnicianIdAsync(techResult.Data.Id);
                        if (ticketsResult.Success && ticketsResult.Data != null)
                        {
                            tickets = ticketsResult.Data;
                        }
                    }
                }
            }
            else if (role == "Cliente")
            {
                var clientIdStr = User.Claims.FirstOrDefault(c => c.Type == "ClientId")?.Value;
                if (Guid.TryParse(clientIdStr, out var clientId))
                {
                    var ticketsResult = await _supportTicketService.GetByClientIdAsync(clientId);
                    if (ticketsResult.Success && ticketsResult.Data != null)
                    {
                        tickets = ticketsResult.Data;
                    }
                }
            }

            // 2. Processar Estatisticas dos Chamados do Contexto do Usuario
            var ticketsList = tickets.ToList();
            viewModel.TotalTickets = ticketsList.Count;
            viewModel.OpenTickets = ticketsList.Count(t => t.Status == 1); // Aberto
            viewModel.InTriageTickets = ticketsList.Count(t => t.Status == 2); // EmTriagem
            viewModel.InDisplacementTickets = ticketsList.Count(t => t.Status == 3); // EmDeslocamento
            viewModel.InServiceTickets = ticketsList.Count(t => t.Status == 4); // EmAtendimento
            viewModel.CompletedTickets = ticketsList.Count(t => t.Status == 5); // Concluido
            viewModel.CancelledTickets = ticketsList.Count(t => t.Status == 6); // Cancelado
            
            // Faturamento apenas de chamados Concluidos
            viewModel.TotalBilling = ticketsList.Where(t => t.Status == 5).Sum(t => t.TotalCost);

            // Listar os chamados mais recentes (limite 8)
            viewModel.RecentTickets = ticketsList
                .OrderByDescending(t => t.CreatedAt)
                .Take(8)
                .ToList();

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar dados do dashboard.");
            return View(new DashboardViewModel());
        }
    }

    [HttpGet]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
