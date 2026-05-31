namespace SGPST.Application.DTOs.SupportTicket;

// DTO para a criacao de um novo chamado/pedido de suporte tecnico pelo cliente
public record CreateSupportTicketDto(
    Guid ClientId, 
    string Title, 
    string Description, 
    int Priority, // 1-Baixa, 2-Media, 3-Alta, 4-Urgente
    int Type      // 1-Remoto, 2-Presencial
);
