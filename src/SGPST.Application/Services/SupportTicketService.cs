using SGPST.Application.DTOs.SupportTicket;
using SGPST.Application.Interfaces;
using SGPST.Domain.Common;
using SGPST.Domain.Entities;
using SGPST.Domain.Interfaces;

namespace SGPST.Application.Services;

// Servico que orquestra a abertura, triagem, atendimento e faturamento de chamados tecnicos, com integracao de mensageria
public class SupportTicketService : ISupportTicketService
{
    private readonly ISupportTicketRepository _ticketRepository;
    private readonly IClientRepository _clientRepository;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IServicePriceRepository _servicePriceRepository;
    private readonly IDisplacementLogRepository _displacementRepository;
    private readonly IMessageBroker _messageBroker;

    public SupportTicketService(
        ISupportTicketRepository ticketRepository,
        IClientRepository clientRepository,
        ITechnicianRepository technicianRepository,
        IServicePriceRepository servicePriceRepository,
        IDisplacementLogRepository displacementRepository,
        IMessageBroker messageBroker)
    {
        _ticketRepository = ticketRepository;
        _clientRepository = clientRepository;
        _technicianRepository = technicianRepository;
        _servicePriceRepository = servicePriceRepository;
        _displacementRepository = displacementRepository;
        _messageBroker = messageBroker;
    }

    public async Task<IAppResult<SupportTicketDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var ticket = await _ticketRepository.GetByIdAsync(id);
            if (ticket == null)
            {
                return AppResult<SupportTicketDto>.Failure($"Chamado com ID {id} nao encontrado.");
            }

            var dto = MapToDto(ticket);
            return AppResult<SupportTicketDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            return AppResult<SupportTicketDto>.Failure("Erro ao buscar chamado por ID.", ex);
        }
    }

    public async Task<IAppResult<SupportTicketDto>> CreateAsync(CreateSupportTicketDto createDto)
    {
        try
        {
            // Valida se o cliente existe
            var client = await _clientRepository.GetByIdAsync(createDto.ClientId);
            if (client == null)
            {
                return AppResult<SupportTicketDto>.Failure("Cliente informado para abertura do chamado nao existe.");
            }

            // Mapeamento seguro de prioridade e tipo a partir de inteiros
            var priority = (TicketPriority)createDto.Priority;
            var type = (TicketType)createDto.Type;

            var ticket = SupportTicket.Create(
                createDto.ClientId,
                createDto.Title,
                createDto.Description,
                priority,
                type
            );

            // Persiste no banco PostgreSQL
            await _ticketRepository.AddAsync(ticket);

            // Envia evento de criacao assincrona para a fila do RabbitMQ
            await _messageBroker.PublishTicketCreatedAsync(ticket);

            var loaded = await _ticketRepository.GetByIdAsync(ticket.Id);
            var dto = MapToDto(loaded ?? ticket);

            return AppResult<SupportTicketDto>.Ok(dto, "Chamado aberto com sucesso e enviado para a fila de triagem.");
        }
        catch (Exception ex)
        {
            return AppResult<SupportTicketDto>.Failure("Erro ao abrir chamado tecnico.", ex);
        }
    }

    public async Task<IAppResult> AssignAttendantAsync(Guid ticketId, Guid attendantId)
    {
        try
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                return AppResult.Failure($"Chamado com ID {ticketId} nao encontrado.");
            }

            ticket.AssignAttendant(attendantId);
            await _ticketRepository.UpdateAsync(ticket);

            return AppResult.Ok("Atendente atribuido ao chamado com sucesso.");
        }
        catch (Exception ex)
        {
            return AppResult.Failure("Erro ao atribuir atendente ao chamado.", ex);
        }
    }

    public async Task<IAppResult> AssignTechnicianAsync(AssignTechnicianDto assignDto)
    {
        try
        {
            var ticket = await _ticketRepository.GetByIdAsync(assignDto.TicketId);
            if (ticket == null)
            {
                return AppResult.Failure($"Chamado com ID {assignDto.TicketId} nao encontrado.");
            }

            var technician = await _technicianRepository.GetByIdAsync(assignDto.TechnicianId);
            if (technician == null || !technician.IsActive)
            {
                return AppResult.Failure("Tecnico designado nao encontrado ou inativo.");
            }

            ticket.AssignTechnician(assignDto.TechnicianId);
            await _ticketRepository.UpdateAsync(ticket);

            return AppResult.Ok("Tecnico designado para o chamado com sucesso.");
        }
        catch (Exception ex)
        {
            return AppResult.Failure("Erro ao designar tecnico para o chamado.", ex);
        }
    }

    public async Task<IAppResult> StartPhysicalServiceAsync(Guid ticketId)
    {
        try
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                return AppResult.Failure($"Chamado com ID {ticketId} nao encontrado.");
            }

            ticket.StartPhysicalService();
            await _ticketRepository.UpdateAsync(ticket);

            return AppResult.Ok("Atendimento presencial iniciado com sucesso.");
        }
        catch (Exception ex)
        {
            return AppResult.Failure("Erro ao iniciar atendimento presencial.", ex);
        }
    }

    public async Task<IAppResult> CompleteTicketAsync(CompleteTicketDto completeDto)
    {
        try
        {
            var ticket = await _ticketRepository.GetByIdAsync(completeDto.TicketId);
            if (ticket == null)
            {
                return AppResult.Failure($"Chamado com ID {completeDto.TicketId} nao encontrado.");
            }

            var servicePrice = await _servicePriceRepository.GetByIdAsync(completeDto.ServicePriceId);
            if (servicePrice == null)
            {
                return AppResult.Failure("Servico selecionado nao cadastrado no catalogo.");
            }

            // Vincula o servico executado ao chamado e calcula faturamento
            ticket.LinkServiceAndBilling(completeDto.ServicePriceId, servicePrice.Price, completeDto.ExtraCost);
            
            // Conclui o chamado
            ticket.Complete();
            await _ticketRepository.UpdateAsync(ticket);

            return AppResult.Ok("Chamado finalizado e faturado com sucesso.");
        }
        catch (Exception ex)
        {
            return AppResult.Failure("Erro ao finalizar chamado.", ex);
        }
    }

    public async Task<IAppResult> CancelTicketAsync(CancelTicketDto cancelDto)
    {
        try
        {
            var ticket = await _ticketRepository.GetByIdAsync(cancelDto.TicketId);
            if (ticket == null)
            {
                return AppResult.Failure($"Chamado com ID {cancelDto.TicketId} nao encontrado.");
            }

            ticket.Cancel(cancelDto.Reason);
            await _ticketRepository.UpdateAsync(ticket);

            return AppResult.Ok("Chamado cancelado com sucesso.");
        }
        catch (Exception ex)
        {
            return AppResult.Failure("Erro ao cancelar chamado.", ex);
        }
    }

    public async Task<IAppResult<IEnumerable<SupportTicketDto>>> GetAllAsync()
    {
        try
        {
            var tickets = await _ticketRepository.GetAllAsync();
            var dtos = tickets.Select(MapToDto);
            return AppResult<IEnumerable<SupportTicketDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return AppResult<IEnumerable<SupportTicketDto>>.Failure("Erro ao obter todos os chamados.", ex);
        }
    }

    public async Task<IAppResult<IEnumerable<SupportTicketDto>>> GetByStatusAsync(int status)
    {
        try
        {
            var ticketStatus = (TicketStatus)status;
            var tickets = await _ticketRepository.GetByStatusAsync(ticketStatus);
            var dtos = tickets.Select(MapToDto);
            return AppResult<IEnumerable<SupportTicketDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return AppResult<IEnumerable<SupportTicketDto>>.Failure("Erro ao listar chamados por status.", ex);
        }
    }

    public async Task<IAppResult<IEnumerable<SupportTicketDto>>> GetByClientIdAsync(Guid clientId)
    {
        try
        {
            var tickets = await _ticketRepository.GetByClientIdAsync(clientId);
            var dtos = tickets.Select(MapToDto);
            return AppResult<IEnumerable<SupportTicketDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return AppResult<IEnumerable<SupportTicketDto>>.Failure("Erro ao obter chamados do cliente.", ex);
        }
    }

    public async Task<IAppResult<IEnumerable<SupportTicketDto>>> GetByTechnicianIdAsync(Guid technicianId)
    {
        try
        {
            var tickets = await _ticketRepository.GetByTechnicianIdAsync(technicianId);
            var dtos = tickets.Select(MapToDto);
            return AppResult<IEnumerable<SupportTicketDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return AppResult<IEnumerable<SupportTicketDto>>.Failure("Erro ao obter chamados do tecnico.", ex);
        }
    }

    public async Task<IAppResult> StartDisplacementAsync(Guid ticketId, string? startLocation)
    {
        try
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                return AppResult.Failure($"Chamado com ID {ticketId} nao encontrado.");
            }

            if (ticket.Type != TicketType.Presencial)
            {
                return AppResult.Failure("Nao e possivel iniciar deslocamento para chamados de tipo Remoto.");
            }

            // Verifica se ja existe deslocamento ativo
            var active = await _displacementRepository.GetActiveLogByTicketIdAsync(ticketId);
            if (active != null)
            {
                return AppResult.Failure("Ja existe um deslocamento em andamento para este chamado.");
            }

            var log = DisplacementLog.Create(ticketId, startLocation);
            await _displacementRepository.AddAsync(log);

            return AppResult.Ok("Deslocamento do tecnico iniciado com sucesso.");
        }
        catch (Exception ex)
        {
            return AppResult.Failure("Erro ao iniciar deslocamento.", ex);
        }
    }

    public async Task<IAppResult> EndDisplacementAsync(Guid ticketId, string? endLocation)
    {
        try
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                return AppResult.Failure($"Chamado com ID {ticketId} nao encontrado.");
            }

            var log = await _displacementRepository.GetActiveLogByTicketIdAsync(ticketId);
            if (log == null)
            {
                return AppResult.Failure("Nenhum deslocamento ativo foi encontrado para este chamado.");
            }

            log.RecordArrival(endLocation);
            await _displacementRepository.UpdateAsync(log);

            // Altera automaticamente o status do chamado para em atendimento presencial
            ticket.StartPhysicalService();
            await _ticketRepository.UpdateAsync(ticket);

            return AppResult.Ok("Deslocamento finalizado e atendimento fisico iniciado com sucesso.");
        }
        catch (Exception ex)
        {
            return AppResult.Failure("Erro ao finalizar deslocamento.", ex);
        }
    }

    private static SupportTicketDto MapToDto(SupportTicket ticket)
    {
        return new SupportTicketDto(
            ticket.Id,
            ticket.ClientId,
            ticket.Client?.Name ?? string.Empty,
            ticket.Title,
            ticket.Description,
            ticket.AttendantId,
            ticket.Attendant?.Username,
            ticket.TechnicianId,
            ticket.Technician?.User?.Username,
            ticket.ServicePriceId,
            ticket.ServicePrice?.Name,
            (int)ticket.Status,
            ticket.Status.ToString(),
            (int)ticket.Priority,
            ticket.Priority.ToString(),
            (int)ticket.Type,
            ticket.Type.ToString(),
            ticket.CreatedAt,
            ticket.ScheduledFor,
            ticket.ClosedAt,
            ticket.TotalCost
        );
    }
}
