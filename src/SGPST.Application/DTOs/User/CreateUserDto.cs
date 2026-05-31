namespace SGPST.Application.DTOs.User;

// DTO para criacao de um novo usuario do sistema
public record CreateUserDto(string Username, string Email, string Password, string Role);
