namespace SGPST.Application.DTOs.SupportTicket;

// DTO para faturar e finalizar um chamado com sucesso
public record CompleteTicketDto(Guid TicketId, Guid ServicePriceId, decimal ExtraCost);
