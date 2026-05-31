namespace SGPST.Application.DTOs.SupportTicket;

// DTO para designar um tecnico para atender a um chamado especifico
public record AssignTechnicianDto(Guid TicketId, Guid TechnicianId);
