using SGPST.Application.DTOs.User;
using SGPST.Domain.Interfaces;

namespace SGPST.Application.Interfaces;

// Interface para o servico de gestao de usuarios
public interface IUserService : IBaseService
{
    Task<IAppResult<UserDto>> GetByIdAsync(Guid id);
    Task<IAppResult<UserDto>> CreateAsync(CreateUserDto createUserDto);
    Task<IAppResult<IEnumerable<UserDto>>> GetAllAsync();
    Task<IAppResult> SetActiveAsync(Guid id, bool active);
}
