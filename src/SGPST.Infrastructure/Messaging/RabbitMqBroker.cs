using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SGPST.Domain.Entities;
using SGPST.Domain.Interfaces;

namespace SGPST.Infrastructure.Messaging;

// Implementacao concreta do broker de mensageria usando RabbitMQ.Client (versao 7.x assincrona)
public class RabbitMqBroker : IMessageBroker, IDisposable
{
    private readonly string _hostname;
    private readonly string _queueName = "sgpst_tickets";
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly object _lock = new();
    private bool _isDisposed;

    public RabbitMqBroker(IConfiguration configuration)
    {
        _hostname = configuration["RabbitMQ:Host"] ?? "localhost";
        try
        {
            InitializeRabbitMqAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RabbitMQ] Falha fatal ao inicializar o construtor: {ex.Message}");
        }
    }

    private async Task InitializeRabbitMqAsync()
    {
        int retryCount = 0;
        bool connected = false;

        while (!connected && retryCount < 5)
        {
            try
            {
                Console.WriteLine($"[RabbitMQ] Tentando conectar em {_hostname} (Tentativa {retryCount + 1})...");
                var factory = new ConnectionFactory() { HostName = _hostname };

                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                // Declara a fila de chamados como duravel e nao exclusiva
                await _channel.QueueDeclareAsync(
                    queue: _queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                connected = true;
                Console.WriteLine("[RabbitMQ] Conectado com sucesso!");
            }
            catch (Exception ex)
            {
                retryCount++;
                Console.WriteLine($"[RabbitMQ] Erro ao conectar (Tentativa {retryCount}): {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[RabbitMQ] Inner Error: {ex.InnerException.Message}");
                }

                if (retryCount < 5)
                {
                    Console.WriteLine("[RabbitMQ] Aguardando 5 segundos para nova tentativa...");
                    await Task.Delay(5000);
                }
            }
        }

        if (!connected)
        {
            Console.WriteLine("[RabbitMQ] NAO FOI POSSIVEL CONECTAR AO RABBITMQ APOS 5 TENTATIVAS.");
            Console.WriteLine("[RabbitMQ] Verifique se o container Docker ou servico local esta rodando.");
        }
    }

    public async Task PublishTicketCreatedAsync(SupportTicket ticket)
    {
        try
        {
            if (_channel == null)
                throw new InvalidOperationException("Canal do RabbitMQ nao inicializado.");

            // Serializa o ticket excluindo propriedades ciclicas ou de navegacao se houver
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };

            var json = JsonSerializer.Serialize(ticket, options);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                Persistent = true // Mensagem persistente para garantir entrega
            };

            await _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _queueName,
                mandatory: true,
                basicProperties: properties,
                body: body
            );

            Console.WriteLine($"[RabbitMQ] Mensagem publicada com sucesso para o chamado {ticket.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RabbitMQ] Erro ao publicar chamado {ticket.Id}: {ex.Message}");
            throw new InvalidOperationException("Erro ao publicar chamado na fila do RabbitMQ.", ex);
        }
    }

    public void SubscribeToTickets(Func<SupportTicket, Task<bool>> onMessageReceived)
    {
        try
        {
            if (_channel == null)
                throw new InvalidOperationException("Canal do RabbitMQ nao inicializado para subscricao.");

            // Prefetch count de 1 para distribuir uniformemente o trabalho entre multiplos consumers
            _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false).GetAwaiter().GetResult();

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    var ticket = JsonSerializer.Deserialize<SupportTicket>(message);
                    if (ticket != null)
                    {
                        var success = await onMessageReceived(ticket);
                        if (success)
                        {
                            await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                        }
                        else
                        {
                            // Re-enfileira a mensagem se o processamento logico falhar
                            await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                        }
                    }
                    else
                    {
                        // Desserializacao nula: rejeita e nao re-enfileira
                        await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RabbitMQ] Erro de processamento na mensagem: {ex.Message}");
                    // Em caso de erro nao tratado, re-enfileira para tentar novamente
                    await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsumeAsync(
                queue: _queueName,
                autoAck: false,
                consumer: consumer
            ).GetAwaiter().GetResult();

            Console.WriteLine($"[RabbitMQ] Subscrito na fila '{_queueName}' com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RabbitMQ] Erro ao subscrever na fila: {ex.Message}");
            throw new InvalidOperationException("Erro de subscricao de mensageria.", ex);
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (_isDisposed) return;
            _isDisposed = true;

            try
            {
                _channel?.CloseAsync().GetAwaiter().GetResult();
                _connection?.CloseAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RabbitMQ] Erro no descarte do broker: {ex.Message}");
            }
        }
    }
}
