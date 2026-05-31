using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SGPST.Application.DTOs.Auth;
using SGPST.Application.DTOs.Client;
using SGPST.Application.DTOs.ServicePrice;
using SGPST.Application.DTOs.SupportTicket;
using SGPST.Application.DTOs.Technician;

namespace SGPST.Presentation.Desktop;

// Cliente de integracao HTTP com a Web API do SGPST Enterprise (padrao Singleton)
public class ApiClient
{
    private static ApiClient? _instance;
    public static ApiClient Instance => _instance ??= new ApiClient();

    private readonly HttpClient _httpClient;
    
    public string? Token { get; private set; }
    public string? Username { get; private set; }
    public string? Role { get; private set; }
    public Guid? UserId { get; private set; }
    public string? UserEmail { get; private set; }

    private ApiClient()
    {
        // Define o endereço base da API REST rodando localmente
        _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5196/api/") };
    }

    private void SetAuthHeader()
    {
        if (!string.IsNullOrEmpty(Token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    // 1. Autenticacao
    public async Task<(bool Success, string Message)> LoginAsync(string username, string password)
    {
        try
        {
            var loginDto = new LoginDto(username, password);
            var response = await _httpClient.PostAsJsonAsync("auth/login", loginDto);

            if (!response.IsSuccessStatusCode)
            {
                var errObj = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                var errMsg = errObj != null && errObj.TryGetValue("message", out var m) ? m : "Acesso nao autorizado.";
                return (false, errMsg);
            }

            var result = await response.Content.ReadFromJsonAsync<TokenResultDto>();
            if (result == null || string.IsNullOrEmpty(result.Token))
            {
                return (false, "Falha ao ler dados de autenticacao.");
            }

            Token = result.Token;
            Username = result.Username;
            Role = result.Role;

            // Decodifica o token JWT para extrair o UserId e Email do usuario logado
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(result.Token);
                var idClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier || c.Type == "sub");
                if (idClaim != null && Guid.TryParse(idClaim.Value, out var parsedId))
                {
                    UserId = parsedId;
                }

                var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email || c.Type == "email");
                if (emailClaim != null)
                {
                    UserEmail = emailClaim.Value;
                }
            }
            catch
            {
                UserId = null;
            }

            SetAuthHeader();

            return (true, "Autenticacao concluida com sucesso.");
        }
        catch (Exception ex)
        {
            return (false, $"Erro de conexao: {ex.Message}");
        }
    }

    public void Logout()
    {
        Token = null;
        Username = null;
        Role = null;
        UserId = null;
        UserEmail = null;
        SetAuthHeader();
    }

    // 2. Chamados (Tickets)
    public async Task<IEnumerable<SupportTicketDto>> GetTicketsAsync()
    {
        try
        {
            // Filtro por perfil do usuario logado implementado via endpoints especificos da API
            if (Role == "Tecnico")
            {
                // Descobrir o ID do Tecnico logado e carregar
                var tech = await GetTechnicianByUserIdAsync();
                if (tech != null)
                {
                    return await _httpClient.GetFromJsonAsync<IEnumerable<SupportTicketDto>>($"tickets/technician/{tech.Id}") ?? new List<SupportTicketDto>();
                }
                return new List<SupportTicketDto>();
            }
            else if (Role == "Cliente")
            {
                // Descobrir o cliente correspondente
                var clients = await GetClientsAsync();
                // Correspondencia por email exato
                var client = clients.FirstOrDefault(c => c.Email.Equals(UserEmail ?? "", StringComparison.OrdinalIgnoreCase));
                if (client != null)
                {
                    return await _httpClient.GetFromJsonAsync<IEnumerable<SupportTicketDto>>($"tickets/client/{client.Id}") ?? new List<SupportTicketDto>();
                }
                return new List<SupportTicketDto>();
            }
            else
            {
                return await _httpClient.GetFromJsonAsync<IEnumerable<SupportTicketDto>>("tickets") ?? new List<SupportTicketDto>();
            }
        }
        catch
        {
            return new List<SupportTicketDto>();
        }
    }

    public async Task<SupportTicketDto?> GetTicketByIdAsync(Guid id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<SupportTicketDto>($"tickets/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool Success, string Message)> CreateTicketAsync(Guid clientId, string title, string description, int priority, int type)
    {
        try
        {
            var createDto = new CreateSupportTicketDto(clientId, title, description, priority, type);
            var response = await _httpClient.PostAsJsonAsync("tickets", createDto);
            return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Chamado aberto com sucesso." : "Erro ao abrir chamado.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string Message)> AssignAttendantAsync(Guid ticketId, Guid attendantId)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"tickets/{ticketId}/assign-attendant", new { attendantId });
            return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Triagem iniciada." : "Erro ao iniciar triagem.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string Message)> AssignTechnicianAsync(Guid ticketId, Guid technicianId)
    {
        try
        {
            var assignDto = new AssignTechnicianDto(ticketId, technicianId);
            var response = await _httpClient.PutAsJsonAsync("tickets/assign-technician", assignDto);
            return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Tecnico designado com sucesso." : "Erro ao designar tecnico.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string Message)> StartDisplacementAsync(Guid ticketId, string location)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"tickets/{ticketId}/start-displacement", new { location });
            return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Saida registrada." : "Erro ao registrar viagem.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string Message)> EndDisplacementAsync(Guid ticketId, string location)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"tickets/{ticketId}/end-displacement", new { location });
            return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Chegada ao local registrada." : "Erro ao registrar chegada.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string Message)> CompleteTicketAsync(Guid ticketId, Guid servicePriceId, decimal extraCost)
    {
        try
        {
            var completeDto = new CompleteTicketDto(ticketId, servicePriceId, extraCost);
            var response = await _httpClient.PutAsJsonAsync("tickets/complete", completeDto);
            return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Chamado concluido." : "Erro ao concluir chamado.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string Message)> CancelTicketAsync(Guid ticketId, string reason)
    {
        try
        {
            var cancelDto = new CancelTicketDto(ticketId, reason);
            var response = await _httpClient.PutAsJsonAsync("tickets/cancel", cancelDto);
            return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Chamado cancelado." : "Erro ao cancelar chamado.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // 3. Clientes (Clients)
    public async Task<IEnumerable<ClientDto>> GetClientsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<ClientDto>>("clients") ?? new List<ClientDto>();
        }
        catch
        {
            return new List<ClientDto>();
        }
    }

    public async Task<(bool Success, string Message)> CreateClientAsync(CreateClientDto createDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("clients", createDto);
            return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Cliente cadastrado." : "Erro ao cadastrar cliente.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // 4. Tecnicos (Technicians)
    public async Task<IEnumerable<TechnicianDto>> GetTechniciansAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<TechnicianDto>>("technicians") ?? new List<TechnicianDto>();
        }
        catch
        {
            return new List<TechnicianDto>();
        }
    }

    public async Task<TechnicianDto?> GetTechnicianByUserIdAsync()
    {
        try
        {
            // Procura o tecnico associado ao usuario logado
            var techs = await GetTechniciansAsync();
            // Precisamos do UserId do usuario logado, para isso buscamos na lista de usuarios ou via email
            // Podemos mapear pelo Username no DTO do tecnico
            return techs.FirstOrDefault(t => t.Username.Equals(Username, StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool Success, string Message)> SetTechnicianAvailabilityAsync(Guid id, bool isAvailable)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"technicians/{id}/availability", new { isAvailable });
            return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Disponibilidade alterada." : "Erro ao alterar disponibilidade.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // 5. Catalogo (ServicePrices)
    public async Task<IEnumerable<ServicePriceDto>> GetServicePricesAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<ServicePriceDto>>("serviceprices") ?? new List<ServicePriceDto>();
        }
        catch
        {
            return new List<ServicePriceDto>();
        }
    }

    public async Task<(bool Success, string Message)> CreateServicePriceAsync(CreateServicePriceDto createDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("serviceprices", createDto);
            return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Servico cadastrado." : "Erro ao cadastrar servico.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<IEnumerable<DisplacementLogDto>> GetDisplacementsAsync(Guid ticketId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<DisplacementLogDto>>($"tickets/{ticketId}/displacements") ?? new List<DisplacementLogDto>();
        }
        catch
        {
            return new List<DisplacementLogDto>();
        }
    }
}

public record DisplacementLogDto(
    Guid Id,
    Guid TicketId,
    DateTime DepartureTime,
    DateTime? ArrivalTime,
    string? StartLocation,
    string? EndLocation
);
