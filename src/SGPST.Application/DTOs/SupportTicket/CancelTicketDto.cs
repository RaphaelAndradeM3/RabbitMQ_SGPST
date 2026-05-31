namespace SGPST.Application.DTOs.SupportTicket;

// DTO contendo a justificativa para o cancelamento de um chamado
public record CancelTicketDto(Guid TicketId, string Reason);
