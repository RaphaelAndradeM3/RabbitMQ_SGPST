using SGPST.Domain.Entities;

namespace SGPST.Domain.Interfaces;

// Interface de contrato para persistencia dos registros de deslocamento de tecnicos
public interface IDisplacementLogRepository
{
    Task<DisplacementLog?> GetByIdAsync(Guid id);
    Task<DisplacementLog?> GetActiveLogByTicketIdAsync(Guid ticketId); // Retorna o log que ainda nao possui horario de chegada registrado
    Task AddAsync(DisplacementLog log);
    Task UpdateAsync(DisplacementLog log);
    Task<IEnumerable<DisplacementLog>> GetByTicketIdAsync(Guid ticketId);
}
