using SGPST.Domain.Entities;

namespace SGPST.Domain.Interfaces;

// Interface de contrato para persistencia da entidade de Tecnico
public interface ITechnicianRepository
{
    Task<Technician?> GetByIdAsync(Guid id);
    Task<Technician?> GetByUserIdAsync(Guid userId);
    Task AddAsync(Technician technician);
    Task UpdateAsync(Technician technician);
    Task<IEnumerable<Technician>> GetAllAsync();
    Task<IEnumerable<Technician>> GetAvailableAsync();
}
