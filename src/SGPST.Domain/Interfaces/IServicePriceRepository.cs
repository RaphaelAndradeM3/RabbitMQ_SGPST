using SGPST.Domain.Entities;

namespace SGPST.Domain.Interfaces;

// Interface de contrato para persistencia do catalogo de Servicos e Precos
public interface IServicePriceRepository
{
    Task<ServicePrice?> GetByIdAsync(Guid id);
    Task AddAsync(ServicePrice servicePrice);
    Task UpdateAsync(ServicePrice servicePrice);
    Task<IEnumerable<ServicePrice>> GetAllAsync();
    Task<IEnumerable<ServicePrice>> GetActiveAsync();
}
