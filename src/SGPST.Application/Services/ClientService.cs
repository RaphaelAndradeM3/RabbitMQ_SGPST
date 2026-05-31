using SGPST.Application.DTOs.Client;
using SGPST.Application.Interfaces;
using SGPST.Domain.Common;
using SGPST.Domain.Entities;
using SGPST.Domain.Interfaces;

namespace SGPST.Application.Services;

// Servico para cadastro e manutencao de clientes
public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;

    public ClientService(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<IAppResult<ClientDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null)
            {
                return AppResult<ClientDto>.Failure($"Cliente com ID {id} nao encontrado.");
            }

            var dto = MapToDto(client);
            return AppResult<ClientDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            return AppResult<ClientDto>.Failure("Erro ao buscar cliente por ID.", ex);
        }
    }

    public async Task<IAppResult<ClientDto>> GetByDocumentAsync(string document)
    {
        try
        {
            var client = await _clientRepository.GetByDocumentAsync(document);
            if (client == null)
            {
                return AppResult<ClientDto>.Failure($"Cliente com o documento {document} nao encontrado.");
            }

            var dto = MapToDto(client);
            return AppResult<ClientDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            return AppResult<ClientDto>.Failure("Erro ao buscar cliente pelo documento.", ex);
        }
    }

    public async Task<IAppResult<ClientDto>> CreateAsync(CreateClientDto createClientDto)
    {
        try
        {
            // Valida se ja existe cliente com este documento
            var existing = await _clientRepository.GetByDocumentAsync(createClientDto.Document);
            if (existing != null)
            {
                return AppResult<ClientDto>.Failure("Ja existe um cliente cadastrado com este CPF/CNPJ.");
            }

            var client = Client.Create(
                createClientDto.Name,
                createClientDto.Document,
                createClientDto.Email,
                createClientDto.Phone,
                createClientDto.AddressLine,
                createClientDto.Neighborhood,
                createClientDto.City,
                createClientDto.State,
                createClientDto.ZipCode
            );

            await _clientRepository.AddAsync(client);

            var dto = MapToDto(client);
            return AppResult<ClientDto>.Ok(dto, "Cliente cadastrado com sucesso.");
        }
        catch (Exception ex)
        {
            return AppResult<ClientDto>.Failure("Erro ao cadastrar novo cliente.", ex);
        }
    }

    public async Task<IAppResult<ClientDto>> UpdateAsync(Guid id, CreateClientDto updateClientDto)
    {
        try
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null)
            {
                return AppResult<ClientDto>.Failure($"Cliente com ID {id} nao encontrado.");
            }

            client.UpdateDetails(
                updateClientDto.Name,
                updateClientDto.Phone,
                updateClientDto.AddressLine,
                updateClientDto.Neighborhood,
                updateClientDto.City,
                updateClientDto.State,
                updateClientDto.ZipCode
            );

            await _clientRepository.UpdateAsync(client);

            var dto = MapToDto(client);
            return AppResult<ClientDto>.Ok(dto, "Dados do cliente atualizados com sucesso.");
        }
        catch (Exception ex)
        {
            return AppResult<ClientDto>.Failure("Erro ao atualizar dados do cliente.", ex);
        }
    }

    public async Task<IAppResult<IEnumerable<ClientDto>>> GetAllAsync()
    {
        try
        {
            var clients = await _clientRepository.GetAllAsync();
            var dtos = clients.Select(MapToDto);
            return AppResult<IEnumerable<ClientDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return AppResult<IEnumerable<ClientDto>>.Failure("Erro ao obter lista de clientes.", ex);
        }
    }

    public async Task<IAppResult> SetActiveAsync(Guid id, bool active)
    {
        try
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null)
            {
                return AppResult.Failure($"Cliente com ID {id} nao encontrado.");
            }

            client.SetActive(active);
            await _clientRepository.UpdateAsync(client);

            var status = active ? "ativado" : "desativado";
            return AppResult.Ok($"Cadastro do cliente {status} com sucesso.");
        }
        catch (Exception ex)
        {
            return AppResult.Failure("Erro ao alterar status de ativacao do cliente.", ex);
        }
    }

    private static ClientDto MapToDto(Client client)
    {
        return new ClientDto(
            client.Id,
            client.Name,
            client.Document,
            client.Email,
            client.Phone,
            client.AddressLine,
            client.Neighborhood,
            client.City,
            client.State,
            client.ZipCode,
            client.IsActive
        );
    }
}
