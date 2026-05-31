using SGPST.Application.DTOs.User;
using SGPST.Application.Interfaces;
using SGPST.Domain.Common;
using SGPST.Domain.Entities;
using SGPST.Domain.Interfaces;

namespace SGPST.Application.Services;

// Servico para cadastro e gerenciamento de usuarios
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IAppResult<UserDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return AppResult<UserDto>.Failure($"Usuario com ID {id} nao encontrado.");
            }

            var dto = MapToDto(user);
            return AppResult<UserDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            return AppResult<UserDto>.Failure("Erro ao obter usuario por ID.", ex);
        }
    }

    public async Task<IAppResult<UserDto>> CreateAsync(CreateUserDto createUserDto)
    {
        try
        {
            // Valida duplicidade de username e email
            var existingUsername = await _userRepository.GetByUsernameAsync(createUserDto.Username);
            if (existingUsername != null)
            {
                return AppResult<UserDto>.Failure("Este nome de usuario ja esta cadastrado.");
            }

            var existingEmail = await _userRepository.GetByEmailAsync(createUserDto.Email);
            if (existingEmail != null)
            {
                return AppResult<UserDto>.Failure("Este email ja esta cadastrado.");
            }

            // Gera hash seguro da senha
            var passwordHash = AuthService.ComputeHash(createUserDto.Password);

            // Cria entidade de dominio
            var user = User.Create(
                createUserDto.Username,
                createUserDto.Email,
                passwordHash,
                createUserDto.Role
            );

            await _userRepository.AddAsync(user);

            var dto = MapToDto(user);
            return AppResult<UserDto>.Ok(dto, "Usuario cadastrado com sucesso.");
        }
        catch (Exception ex)
        {
            return AppResult<UserDto>.Failure("Erro ao cadastrar novo usuario.", ex);
        }
    }

    public async Task<IAppResult<IEnumerable<UserDto>>> GetAllAsync()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            var dtos = users.Select(MapToDto);
            return AppResult<IEnumerable<UserDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return AppResult<IEnumerable<UserDto>>.Failure("Erro ao obter lista de usuarios.", ex);
        }
    }

    public async Task<IAppResult> SetActiveAsync(Guid id, bool active)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return AppResult.Failure($"Usuario com ID {id} nao encontrado.");
            }

            user.SetActive(active);
            await _userRepository.UpdateAsync(user);

            var status = active ? "ativado" : "desativado";
            return AppResult.Ok($"Usuario {status} com sucesso.");
        }
        catch (Exception ex)
        {
            return AppResult.Failure("Erro ao alterar status de ativacao do usuario.", ex);
        }
    }

    public async Task<IAppResult> AssociateClientAsync(Guid userId, Guid? clientId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return AppResult.Failure($"Usuario com ID {userId} nao encontrado.");
            }

            user.AssociateClient(clientId);
            await _userRepository.UpdateAsync(user);

            return AppResult.Ok("Associacao de cliente atualizada com sucesso.");
        }
        catch (Exception ex)
        {
            return AppResult.Failure("Erro ao associar cliente ao usuario.", ex);
        }
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto(
            user.Id,
            user.Username,
            user.Email,
            user.Role,
            user.IsActive,
            user.ClientId
        );
    }
}
