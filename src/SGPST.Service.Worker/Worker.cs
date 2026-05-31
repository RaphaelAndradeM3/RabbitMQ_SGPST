using SGPST.Domain.Entities;
using SGPST.Domain.Interfaces;

namespace SGPST.Service.Worker;

// Classe BackgroundService que consome de forma assincrona a fila de chamados do RabbitMQ
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMessageBroker _messageBroker;
    private readonly IServiceScopeFactory _scopeFactory;

    public Worker(ILogger<Worker> logger, IMessageBroker messageBroker, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _messageBroker = messageBroker;
        _scopeFactory = scopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("[Worker] Inicializando subscricao de mensageria na fila...");

            // Subscreve-se na fila de chamados do RabbitMQ
            _messageBroker.SubscribeToTickets(async (ticket) =>
            {
                _logger.LogInformation("[Worker] Novo chamado recebido da fila para triagem automatica. ID: {TicketId}, Titulo: {Title}", ticket.Id, ticket.Title);

                try
                {
                    // Como o Worker e um servico Singleton, criamos um escopo para instanciar os repositorios Scoped do banco
                    using var scope = _scopeFactory.CreateScope();
                    var ticketRepository = scope.ServiceProvider.GetRequiredService<ISupportTicketRepository>();

                    // Busca o chamado no banco PostgreSQL para verificar integridade e carregar dados adicionais
                    var dbTicket = await ticketRepository.GetByIdAsync(ticket.Id);

                    if (dbTicket == null)
                    {
                        _logger.LogWarning("[Worker] Chamado {TicketId} nao encontrado no banco de dados PostgreSQL. Rejeitando mensagem.", ticket.Id);
                        return false; // Falha no processamento (Nack - ira re-enfileirar)
                    }

                    _logger.LogInformation("[Worker] Chamado localizado no banco. Descricao: {Description}", dbTicket.Description);
                    
                    // Simula analise do chamado por um motor de triagem automatica no background
                    await Task.Delay(2000, stoppingToken);

                    _logger.LogInformation("[Worker] Triagem automatica concluida com sucesso para o chamado {TicketId}. Prontificado para atendimento.", dbTicket.Id);
                    
                    return true; // Sucesso no processamento (Ack - retira da fila)
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Worker] Erro ao processar o chamado {TicketId} no background.", ticket.Id);
                    return false; // Falha no processamento (Nack - ira re-enfileirar)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[Worker] Erro critico ao assinar consumo de filas do RabbitMQ.");
        }

        return Task.CompletedTask;
    }
}
