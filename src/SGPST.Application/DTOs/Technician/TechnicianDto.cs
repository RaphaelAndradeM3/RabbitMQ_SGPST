namespace SGPST.Application.DTOs.Technician;

// DTO contendo os dados do tecnico cadastrado no sistema
public record TechnicianDto(
    Guid Id,
    Guid UserId,
    string Username,
    string Email,
    string Specialty,
    bool IsAvailable,
    string? CurrentLocation,
    bool IsActive
);
