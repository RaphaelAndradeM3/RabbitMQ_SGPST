namespace SGPST.Application.DTOs.Auth;

// DTO de retorno apos a autenticacao bem sucedida
public record TokenResultDto(string Token, string Username, string Role, DateTime ExpiresAt);
