namespace SGPST.Application.DTOs.User;

// DTO para representacao das informacoes do usuario
public record UserDto(Guid Id, string Username, string Email, string Role, bool IsActive, Guid? ClientId = null);
