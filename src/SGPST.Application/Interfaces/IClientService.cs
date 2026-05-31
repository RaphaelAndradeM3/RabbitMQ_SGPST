using SGPST.Application.DTOs.Client;
using SGPST.Domain.Interfaces;

namespace SGPST.Application.Interfaces;

// Interface para o servico de gestao de clientes
public interface IClientService : IBaseService
{
    Task<IAppResult<ClientDto>> GetByIdAsync(Guid id);
    Task<IAppResult<ClientDto>> GetByDocumentAsync(string document);
    Task<IAppResult<ClientDto>> CreateAsync(CreateClientDto createClientDto);
    Task<IAppResult<ClientDto>> UpdateAsync(Guid id, CreateClientDto updateClientDto);
    Task<IAppResult<IEnumerable<ClientDto>>> GetAllAsync();
    Task<IAppResult> SetActiveAsync(Guid id, bool active);
}
