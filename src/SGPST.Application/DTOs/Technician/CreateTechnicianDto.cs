namespace SGPST.Application.DTOs.Technician;

// DTO para cadastrar um novo tecnico vinculando a um usuario
public record CreateTechnicianDto(Guid UserId, string Specialty);
