using SGPST.Domain.Entities;

namespace SGPST.Domain.Interfaces;

// Interface de contrato para persistencia da entidade de Chamado (SupportTicket)
public interface ISupportTicketRepository
{
    Task<SupportTicket?> GetByIdAsync(Guid id);
    Task AddAsync(SupportTicket ticket);
    Task UpdateAsync(SupportTicket ticket);
    Task<IEnumerable<SupportTicket>> GetAllAsync();
    Task<IEnumerable<SupportTicket>> GetByStatusAsync(TicketStatus status);
    Task<IEnumerable<SupportTicket>> GetByClientIdAsync(Guid clientId);
    Task<IEnumerable<SupportTicket>> GetByTechnicianIdAsync(Guid technicianId);
}
