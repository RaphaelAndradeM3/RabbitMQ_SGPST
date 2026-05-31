using SGPST.Application.DTOs.ServicePrice;
using SGPST.Application.Interfaces;
using SGPST.Domain.Common;
using SGPST.Domain.Entities;
using SGPST.Domain.Interfaces;

namespace SGPST.Application.Services;

// Servico para cadastro e manutencao de precos de servicos no catalogo
public class ServicePriceService : IServicePriceService
{
    private readonly IServicePriceRepository _servicePriceRepository;

    public ServicePriceService(IServicePriceRepository servicePriceRepository)
    {
        _servicePriceRepository = servicePriceRepository;
    }

    public async Task<IAppResult<ServicePriceDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var servicePrice = await _servicePriceRepository.GetByIdAsync(id);
            if (servicePrice == null)
            {
                return AppResult<ServicePriceDto>.Failure($"Servico com ID {id} nao encontrado no catalogo.");
            }

            var dto = MapToDto(servicePrice);
            return AppResult<ServicePriceDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            return AppResult<ServicePriceDto>.Failure("Erro ao buscar servico por ID.", ex);
        }
    }

    public async Task<IAppResult<ServicePriceDto>> CreateAsync(CreateServicePriceDto createDto)
    {
        try
        {
            var servicePrice = ServicePrice.Create(
                createDto.Name,
                createDto.Description,
                createDto.Price
            );

            await _servicePriceRepository.AddAsync(servicePrice);

            var dto = MapToDto(servicePrice);
            return AppResult<ServicePriceDto>.Ok(dto, "Servico cadastrado no catalogo com sucesso.");
        }
        catch (Exception ex)
        {
            return AppResult<ServicePriceDto>.Failure("Erro ao cadastrar servico no catalogo.", ex);
        }
    }

    public async Task<IAppResult<ServicePriceDto>> UpdateAsync(Guid id, CreateServicePriceDto updateDto)
    {
        try
        {
            var servicePrice = await _servicePriceRepository.GetByIdAsync(id);
            if (servicePrice == null)
            {
                return AppResult<ServicePriceDto>.Failure($"Servico com ID {id} nao encontrado no catalogo.");
            }

            servicePrice.UpdateService(updateDto.Name, updateDto.Description, updateDto.Price);
            await _servicePriceRepository.UpdateAsync(servicePrice);

            var dto = MapToDto(servicePrice);
            return AppResult<ServicePriceDto>.Ok(dto, "Dados do servico atualizados com sucesso.");
        }
        catch (Exception ex)
        {
            return AppResult<ServicePriceDto>.Failure("Erro ao atualizar dados do servico.", ex);
        }
    }

    public async Task<IAppResult<IEnumerable<ServicePriceDto>>> GetAllAsync()
    {
        try
        {
            var services = await _servicePriceRepository.GetAllAsync();
            var dtos = services.Select(MapToDto);
            return AppResult<IEnumerable<ServicePriceDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return AppResult<IEnumerable<ServicePriceDto>>.Failure("Erro ao obter catalogo de servicos.", ex);
        }
    }

    public async Task<IAppResult<IEnumerable<ServicePriceDto>>> GetActiveAsync()
    {
        try
        {
            var services = await _servicePriceRepository.GetActiveAsync();
            var dtos = services.Select(MapToDto);
            return AppResult<IEnumerable<ServicePriceDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return AppResult<IEnumerable<ServicePriceDto>>.Failure("Erro ao obter catalogo de servicos ativos.", ex);
        }
    }

    public async Task<IAppResult> SetActiveAsync(Guid id, bool active)
    {
        try
        {
            var servicePrice = await _servicePriceRepository.GetByIdAsync(id);
            if (servicePrice == null)
            {
                return AppResult.Failure($"Servico com ID {id} nao encontrado no catalogo.");
            }

            servicePrice.SetActive(active);
            await _servicePriceRepository.UpdateAsync(servicePrice);

            var status = active ? "ativado" : "desativado";
            return AppResult.Ok($"Servico {status} com sucesso no catalogo.");
        }
        catch (Exception ex)
        {
            return AppResult.Failure("Erro ao alterar status de ativacao do servico.", ex);
        }
    }

    private static ServicePriceDto MapToDto(ServicePrice service)
    {
        return new ServicePriceDto(
            service.Id,
            service.Name,
            service.Description,
            service.Price,
            service.IsActive
        );
    }
}
