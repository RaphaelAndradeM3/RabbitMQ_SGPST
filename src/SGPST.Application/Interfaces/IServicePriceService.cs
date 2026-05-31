using SGPST.Application.DTOs.ServicePrice;
using SGPST.Domain.Interfaces;

namespace SGPST.Application.Interfaces;

// Interface para o servico de catalogo de servicos e precos
public interface IServicePriceService : IBaseService
{
    Task<IAppResult<ServicePriceDto>> GetByIdAsync(Guid id);
    Task<IAppResult<ServicePriceDto>> CreateAsync(CreateServicePriceDto createServicePriceDto);
    Task<IAppResult<ServicePriceDto>> UpdateAsync(Guid id, CreateServicePriceDto updateServicePriceDto);
    Task<IAppResult<IEnumerable<ServicePriceDto>>> GetAllAsync();
    Task<IAppResult<IEnumerable<ServicePriceDto>>> GetActiveAsync();
    Task<IAppResult> SetActiveAsync(Guid id, bool active);
}
