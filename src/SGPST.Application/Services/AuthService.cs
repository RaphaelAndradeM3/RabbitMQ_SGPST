using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SGPST.Application.DTOs.Auth;
using SGPST.Application.Interfaces;
using SGPST.Domain.Common;
using SGPST.Domain.Interfaces;

namespace SGPST.Application.Services;

// Servico de autenticacao responsavel por gerar tokens JWT e verificar senhas com hash SHA-256
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly string _jwtSecret;
    private readonly int _jwtExpirationHours;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        
        // Garante uma chave segura com tamanho minimo exigido pelo HS256 (32 bytes / 256 bits)
        _jwtSecret = configuration["Jwt:SecretKey"] ?? "SGPST_Enterprise_Super_Secret_Key_2026_RSA_Security_Token";
        
        if (int.TryParse(configuration["Jwt:ExpirationHours"], out var hours))
        {
            _jwtExpirationHours = hours;
        }
        else
        {
            _jwtExpirationHours = 8; // Expira em 8 horas por padrao
        }
    }

    public async Task<IAppResult<TokenResultDto>> LoginAsync(LoginDto loginDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(loginDto.Username) || string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return AppResult<TokenResultDto>.Failure("Nome de usuario e senha sao obrigatorios.");
            }

            var user = await _userRepository.GetByUsernameAsync(loginDto.Username.Trim());
            
            if (user == null || !user.IsActive)
            {
                return AppResult<TokenResultDto>.Failure("Usuario nao encontrado ou inativo.");
            }

            // Calcula o hash da senha enviada para comparacao
            var computedHash = ComputeHash(loginDto.Password);
            
            if (user.PasswordHash != computedHash)
            {
                return AppResult<TokenResultDto>.Failure("Credenciais invalidas.");
            }

            // Geração manual de token JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);
            var expiresAt = DateTime.UtcNow.AddHours(_jwtExpirationHours);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };
            if (user.ClientId.HasValue)
            {
                claims.Add(new Claim("ClientId", user.ClientId.Value.ToString()));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAt,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            var result = new TokenResultDto(tokenString, user.Username, user.Role, expiresAt);
            return AppResult<TokenResultDto>.Ok(result, "Autenticacao realizada com sucesso.");
        }
        catch (Exception ex)
        {
            return AppResult<TokenResultDto>.Failure("Erro interno durante o processamento do login.", ex);
        }
    }

    // Calcula o hash SHA-256 de uma string para armazenamento seguro de senhas
    public static string ComputeHash(string password)
    {
        try
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            
            var builder = new StringBuilder();
            foreach (var b in hash)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro critico ao gerar hash de senha.", ex);
        }
    }
}
