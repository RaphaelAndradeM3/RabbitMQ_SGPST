using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SGPST.Domain.Entities;
using SGPST.Domain.Interfaces;

namespace SGPST.Infrastructure.Messaging;

public class RabbitMqBroker : IMessageBroker, IDisposable
{
    private readonly string _hostname;
    private readonly string _queueName = "sgpst_orders";
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqBroker(string hostname = "localhost")
    {
        _hostname = hostname;
        InitializeRabbitMq();
    }

    private void InitializeRabbitMq()
    {
        int retryCount = 0;
        bool connected = false;

        while (!connected && retryCount < 5)
        {
            try
            {
                Console.WriteLine($"[RabbitMQ] Tentando conectar em {_hostname} (Tentativa {retryCount + 1})...");
                var factory = new ConnectionFactory() { HostName = _hostname };
                
                // Forcando o uso de localhost se estiver vazio
                if (string.IsNullOrEmpty(factory.HostName)) factory.HostName = "localhost";

                _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
                _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

                _channel.QueueDeclareAsync(queue: _queueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null).GetAwaiter().GetResult();
                
                connected = true;
                Console.WriteLine("[RabbitMQ] Conectado com sucesso!");
            }
            catch (Exception ex)
            {
                retryCount++;
                Console.WriteLine($"[RabbitMQ] Erro ao conectar (Tentativa {retryCount}): {ex.Message}");
                if (ex.InnerException != null) 
                    Console.WriteLine($"[RabbitMQ] Inner Error: {ex.InnerException.Message}");
                
                if (retryCount < 5)
                {
                    Console.WriteLine("[RabbitMQ] Aguardando 5 segundos para nova tentativa...");
                    Thread.Sleep(5000);
                }
            }
        }

        if (!connected)
        {
            Console.WriteLine("[RabbitMQ] NAO FOI POSSIVEL CONECTAR AO RABBITMQ APOS 5 TENTATIVAS.");
            Console.WriteLine("[RabbitMQ] Verifique se o container Docker esta rodando (start-rabbitmq.bat).");
        }
    }

    public async Task PublishOrderAsync(Order order)
    {
        try
        {
            if (_channel == null) return;

            var json = JsonSerializer.Serialize(order);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                Persistent = true
            };

            await _channel.BasicPublishAsync(exchange: string.Empty,
                                       routingKey: _queueName,
                                       mandatory: true,
                                       basicProperties: properties,
                                       body: body);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao publicar pedido no RabbitMQ: {ex.Message}");
            throw;
        }
    }

    public void SubscribeToOrders(Func<Order, Task<bool>> onMessageReceived)
    {
        try
        {
            if (_channel == null) return;

            // Define QoS para processamento proporcional (1 mensagem por vez)
            _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false).GetAwaiter().GetResult();

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                
                try
                {
                    var order = JsonSerializer.Deserialize<Order>(message);
                    if (order != null)
                    {
                        var success = await onMessageReceived(order);
                        if (success)
                        {
                            await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                        }
                        else
                        {
                            // Re-enfileira em caso de falha no processamento logico
                            await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao processar mensagem da fila: {ex.Message}");
                    // Re-enfileira em caso de erro de desserializacao ou erro fatal
                    await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsumeAsync(queue: _queueName,
                                 autoAck: false,
                                 consumer: consumer).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao subscrever as filas do RabbitMQ: {ex.Message}");
        }
    }

    public void Dispose()
    {
        try
        {
            _channel?.CloseAsync().GetAwaiter().GetResult();
            _connection?.CloseAsync().GetAwaiter().GetResult();
        }
        catch { }
    }
}
