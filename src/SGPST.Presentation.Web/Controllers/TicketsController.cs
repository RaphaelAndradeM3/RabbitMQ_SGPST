using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPST.Application.DTOs.SupportTicket;
using SGPST.Application.Interfaces;
using SGPST.Domain.Interfaces;

namespace SGPST.Presentation.Web.Controllers;

[Authorize]
public class TicketsController : Controller
{
    private readonly ISupportTicketService _supportTicketService;
    private readonly IClientService _clientService;
    private readonly ITechnicianService _technicianService;
    private readonly IServicePriceService _servicePriceService;
    private readonly IDisplacementLogRepository _displacementLogRepository;
    private readonly ILogger<TicketsController> _logger;

    public TicketsController(
        ISupportTicketService supportTicketService,
        IClientService clientService,
        ITechnicianService technicianService,
        IServicePriceService servicePriceService,
        IDisplacementLogRepository displacementLogRepository,
        ILogger<TicketsController> logger)
    {
        _supportTicketService = supportTicketService;
        _clientService = clientService;
        _technicianService = technicianService;
        _servicePriceService = servicePriceService;
        _displacementLogRepository = displacementLogRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? statusFilter)
    {
        try
        {
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? string.Empty;
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty;
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            IEnumerable<SupportTicketDto> tickets = new List<SupportTicketDto>();

            // 1. Carregar chamados com base no perfil do usuario logado
            if (role == "Admin" || role == "Atendente")
            {
                var result = await _supportTicketService.GetAllAsync();
                if (result.Success && result.Data != null)
                {
                    tickets = result.Data;
                }
            }
            else if (role == "Tecnico")
            {
                if (Guid.TryParse(userIdStr, out var userId))
                {
                    var techResult = await _technicianService.GetByUserIdAsync(userId);
                    if (techResult.Success && techResult.Data != null)
                    {
                        var result = await _supportTicketService.GetByTechnicianIdAsync(techResult.Data.Id);
                        if (result.Success && result.Data != null)
                        {
                            tickets = result.Data;
                        }
                    }
                }
            }
            else if (role == "Cliente")
            {
                var clientsResult = await _clientService.GetAllAsync();
                if (clientsResult.Success && clientsResult.Data != null)
                {
                    var client = clientsResult.Data.FirstOrDefault(c => c.Email.Equals(userEmail, StringComparison.OrdinalIgnoreCase));
                    if (client != null)
                    {
                        var result = await _supportTicketService.GetByClientIdAsync(client.Id);
                        if (result.Success && result.Data != null)
                        {
                            tickets = result.Data;
                        }
                    }
                }
            }

            // 2. Aplicar filtro de status se selecionado
            if (statusFilter.HasValue && statusFilter.Value > 0)
            {
                tickets = tickets.Where(t => t.Status == statusFilter.Value);
            }

            ViewData["StatusFilter"] = statusFilter;
            return View(tickets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar chamados.");
            return View(new List<SupportTicketDto>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var ticketResult = await _supportTicketService.GetByIdAsync(id);
            if (!ticketResult.Success || ticketResult.Data == null)
            {
                TempData["ErrorMessage"] = "Chamado nao encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var ticket = ticketResult.Data;
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? string.Empty;
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty;
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            // Restricao de acesso basica para Cliente
            if (role == "Cliente")
            {
                var clientsResult = await _clientService.GetAllAsync();
                var client = clientsResult.Data?.FirstOrDefault(c => c.Email.Equals(userEmail, StringComparison.OrdinalIgnoreCase));
                if (client == null || ticket.ClientId != client.Id)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }
            }
            // Restricao de acesso basica para Tecnico
            else if (role == "Tecnico")
            {
                if (Guid.TryParse(userIdStr, out var userId))
                {
                    var techResult = await _technicianService.GetByUserIdAsync(userId);
                    if (techResult.Success && techResult.Data != null)
                    {
                        if (ticket.TechnicianId.HasValue && ticket.TechnicianId.Value != techResult.Data.Id)
                        {
                            return RedirectToAction("AccessDenied", "Account");
                        }
                    }
                }
            }

            // 1. Carregar logs de deslocamento
            var displacementLogs = await _displacementLogRepository.GetByTicketIdAsync(id);
            ViewBag.DisplacementLogs = displacementLogs;

            // 2. Carregar Tecnicos disponiveis (para triagem se status for Aberto/EmTriagem)
            if (ticket.Status <= 2)
            {
                if (role == "Admin" || role == "Atendente")
                {
                    var techsResult = await _technicianService.GetAvailableAsync();
                    ViewBag.AvailableTechnicians = techsResult.Data ?? new List<Application.DTOs.Technician.TechnicianDto>();
                }
                else if (role == "Tecnico")
                {
                    if (Guid.TryParse(userIdStr, out var userId))
                    {
                        var techResult = await _technicianService.GetByUserIdAsync(userId);
                        if (techResult.Success && techResult.Data != null)
                        {
                            ViewBag.CurrentTechnicianId = techResult.Data.Id;
                        }
                    }
                }
            }

            // 3. Carregar catalogo de precos (para faturamento se status for EmAtendimento e role for Admin/Atendente/Tecnico)
            if (ticket.Status == 4 && (role == "Admin" || role == "Atendente" || role == "Tecnico"))
            {
                var pricesResult = await _servicePriceService.GetActiveAsync();
                ViewBag.ServicePrices = pricesResult.Data ?? new List<Application.DTOs.ServicePrice.ServicePriceDto>();
            }

            return View(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar detalhes do chamado {Id}.", id);
            TempData["ErrorMessage"] = $"Erro interno ao carregar detalhes: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        try
        {
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? string.Empty;
            
            // Se nao for cliente, carregar lista de clientes para selecao no Dropdown
            if (role != "Cliente")
            {
                var clientsResult = await _clientService.GetAllAsync();
                ViewBag.Clients = clientsResult.Data ?? new List<Application.DTOs.Client.ClientDto>();
            }
            
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar formulario de novo chamado.");
            return View();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Guid? clientId, string title, string description, int priority, int type)
    {
        try
        {
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? string.Empty;
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty;
            
            Guid finalClientId = Guid.Empty;

            if (role == "Cliente")
            {
                var clientsResult = await _clientService.GetAllAsync();
                var client = clientsResult.Data?.FirstOrDefault(c => c.Email.Equals(userEmail, StringComparison.OrdinalIgnoreCase));
                if (client == null)
                {
                    ModelState.AddModelError(string.Empty, "Nao foi possivel localizar seu cadastro de cliente.");
                    return View();
                }
                finalClientId = client.Id;
            }
            else
            {
                if (!clientId.HasValue || clientId.Value == Guid.Empty)
                {
                    ModelState.AddModelError(nameof(clientId), "Selecione um cliente.");
                    var clientsResult = await _clientService.GetAllAsync();
                    ViewBag.Clients = clientsResult.Data ?? new List<Application.DTOs.Client.ClientDto>();
                    return View();
                }
                finalClientId = clientId.Value;
            }

            var createDto = new CreateSupportTicketDto(finalClientId, title, description, priority, type);
            var result = await _supportTicketService.CreateAsync(createDto);

            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                if (role != "Cliente")
                {
                    var clientsResult = await _clientService.GetAllAsync();
                    ViewBag.Clients = clientsResult.Data ?? new List<Application.DTOs.Client.ClientDto>();
                }
                return View();
            }

            TempData["SuccessMessage"] = "Chamado aberto com sucesso! Ele foi enviado para triagem.";
            return RedirectToAction(nameof(Details), new { id = result.Data.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao abrir chamado.");
            ModelState.AddModelError(string.Empty, $"Erro interno: {ex.Message}");
            return View();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignAttendant(Guid ticketId)
    {
        try
        {
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out var attendantId))
            {
                TempData["ErrorMessage"] = "Usuario nao identificado para assumir chamado.";
                return RedirectToAction(nameof(Details), new { id = ticketId });
            }

            var result = await _supportTicketService.AssignAttendantAsync(ticketId, attendantId);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = "Chamado colocado em triagem com sucesso.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao assumir chamado {Id}.", ticketId);
            TempData["ErrorMessage"] = $"Erro interno: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id = ticketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignTechnician(Guid ticketId, Guid technicianId)
    {
        try
        {
            var assignDto = new AssignTechnicianDto(ticketId, technicianId);
            var result = await _supportTicketService.AssignTechnicianAsync(assignDto);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = "Tecnico designado para o chamado.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao designar tecnico para chamado {Id}.", ticketId);
            TempData["ErrorMessage"] = $"Erro interno: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id = ticketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartDisplacement(Guid ticketId, string? startLocation)
    {
        try
        {
            var result = await _supportTicketService.StartDisplacementAsync(ticketId, startLocation ?? "Base Operacional");
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = "Inicio de deslocamento registrado.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao iniciar deslocamento do chamado {Id}.", ticketId);
            TempData["ErrorMessage"] = $"Erro interno: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id = ticketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EndDisplacement(Guid ticketId, string? endLocation)
    {
        try
        {
            var result = await _supportTicketService.EndDisplacementAsync(ticketId, endLocation ?? "Local do Cliente");
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = "Chegada ao local registrada. Chamado em atendimento.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao encerrar deslocamento do chamado {Id}.", ticketId);
            TempData["ErrorMessage"] = $"Erro interno: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id = ticketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartPhysicalService(Guid ticketId)
    {
        try
        {
            var result = await _supportTicketService.StartPhysicalServiceAsync(ticketId);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = "Atendimento fisico iniciado.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao iniciar atendimento fisico do chamado {Id}.", ticketId);
            TempData["ErrorMessage"] = $"Erro interno: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id = ticketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(Guid ticketId, Guid servicePriceId, decimal extraCost)
    {
        try
        {
            var completeDto = new CompleteTicketDto(ticketId, servicePriceId, extraCost);
            var result = await _supportTicketService.CompleteTicketAsync(completeDto);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = "Chamado concluido e faturado com sucesso.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao concluir chamado {Id}.", ticketId);
            TempData["ErrorMessage"] = $"Erro interno: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id = ticketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid ticketId, string reason)
    {
        try
        {
            var cancelDto = new CancelTicketDto(ticketId, reason);
            var result = await _supportTicketService.CancelTicketAsync(cancelDto);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = "Chamado cancelado com sucesso.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar chamado {Id}.", ticketId);
            TempData["ErrorMessage"] = $"Erro interno: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id = ticketId });
    }
}
