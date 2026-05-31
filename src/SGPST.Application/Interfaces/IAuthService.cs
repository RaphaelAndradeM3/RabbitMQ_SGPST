using SGPST.Application.DTOs.Auth;
using SGPST.Domain.Interfaces;

namespace SGPST.Application.Interfaces;

// Interface para o servico de autenticacao de usuarios
public interface IAuthService : IBaseService
{
    // Realiza o login do usuario e retorna o token JWT
    Task<IAppResult<TokenResultDto>> LoginAsync(LoginDto loginDto);
}
