namespace SGPST.Application.DTOs.SupportTicket;

// DTO para representar todas as informacoes detalhadas de um chamado de suporte tecnico
public record SupportTicketDto(
    Guid Id,
    Guid ClientId,
    string ClientName,
    string Title,
    string Description,
    Guid? AttendantId,
    string? AttendantName,
    Guid? TechnicianId,
    string? TechnicianName,
    Guid? ServicePriceId,
    string? ServicePriceName,
    int Status,
    string StatusDescription,
    int Priority,
    string PriorityDescription,
    int Type,
    string TypeDescription,
    DateTime CreatedAt,
    DateTime? ScheduledFor,
    DateTime? ClosedAt,
    decimal TotalCost
);
