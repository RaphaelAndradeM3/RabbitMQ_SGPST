using SGPST.Domain.Entities;

namespace SGPST.Domain.Interfaces;

// Interface de contrato para o broker de mensageria (RabbitMQ)
public interface IMessageBroker
{
    // Publica um evento de criacao de chamado na fila
    Task PublishTicketCreatedAsync(SupportTicket ticket);
    
    // Subscreve para processar os chamados recebidos da fila
    void SubscribeToTickets(Func<SupportTicket, Task<bool>> onMessageReceived);
}
