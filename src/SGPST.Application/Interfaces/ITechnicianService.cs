using SGPST.Application.DTOs.Technician;
using SGPST.Domain.Interfaces;

namespace SGPST.Application.Interfaces;

// Interface para o servico de gestao de tecnicos de suporte
public interface ITechnicianService : IBaseService
{
    Task<IAppResult<TechnicianDto>> GetByIdAsync(Guid id);
    Task<IAppResult<TechnicianDto>> GetByUserIdAsync(Guid userId);
    Task<IAppResult<TechnicianDto>> CreateAsync(CreateTechnicianDto createTechnicianDto);
    Task<IAppResult<IEnumerable<TechnicianDto>>> GetAllAsync();
    Task<IAppResult<IEnumerable<TechnicianDto>>> GetAvailableAsync();
    Task<IAppResult> UpdateLocationAsync(Guid id, string location);
    Task<IAppResult> SetAvailabilityAsync(Guid id, bool isAvailable);
    Task<IAppResult> SetActiveAsync(Guid id, bool active);
}
