using SGPST.Application.DTOs.SupportTicket;
using SGPST.Domain.Interfaces;

namespace SGPST.Application.Interfaces;

// Interface para o servico de chamados/pedidos de suporte tecnico (SupportTicket)
public interface ISupportTicketService : IBaseService
{
    Task<IAppResult<SupportTicketDto>> GetByIdAsync(Guid id);
    Task<IAppResult<SupportTicketDto>> CreateAsync(CreateSupportTicketDto createDto);
    Task<IAppResult> AssignAttendantAsync(Guid ticketId, Guid attendantId);
    Task<IAppResult> AssignTechnicianAsync(AssignTechnicianDto assignDto);
    Task<IAppResult> StartPhysicalServiceAsync(Guid ticketId);
    Task<IAppResult> CompleteTicketAsync(CompleteTicketDto completeDto);
    Task<IAppResult> CancelTicketAsync(CancelTicketDto cancelDto);
    
    Task<IAppResult<IEnumerable<SupportTicketDto>>> GetAllAsync();
    Task<IAppResult<IEnumerable<SupportTicketDto>>> GetByStatusAsync(int status);
    Task<IAppResult<IEnumerable<SupportTicketDto>>> GetByClientIdAsync(Guid clientId);
    Task<IAppResult<IEnumerable<SupportTicketDto>>> GetByTechnicianIdAsync(Guid technicianId);

    // Registro de deslocamento
    Task<IAppResult> StartDisplacementAsync(Guid ticketId, string? startLocation);
    Task<IAppResult> EndDisplacementAsync(Guid ticketId, string? endLocation);
}
