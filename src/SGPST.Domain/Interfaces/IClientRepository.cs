using SGPST.Domain.Entities;

namespace SGPST.Domain.Interfaces;

// Interface de contrato para persistencia da entidade de Cliente
public interface IClientRepository
{
    Task<Client?> GetByIdAsync(Guid id);
    Task<Client?> GetByDocumentAsync(string document);
    Task AddAsync(Client client);
    Task UpdateAsync(Client client);
    Task<IEnumerable<Client>> GetAllAsync();
}
