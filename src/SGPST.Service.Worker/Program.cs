using SGPST.Domain.Interfaces;
using SGPST.Infrastructure.Data;
using SGPST.Infrastructure.Messaging;
using SGPST.Infrastructure.Repositories;

Console.WriteLine("Iniciando Worker de Processamento de Pedidos...");

var providerId = $"Prestador-{Guid.NewGuid().ToString().Substring(0, 8)}";
var dbFactory = new SqliteConnectionFactory();

// Garantir que o banco e as tabelas existam no contexto do Worker
dbFactory.SetupDatabase();

var orderRepository = new OrderRepository(dbFactory);
var broker = new RabbitMqBroker("localhost");

Console.WriteLine($"Conectado como {providerId}");

broker.SubscribeToOrders(async (order) =>
{
    try
    {
        Console.WriteLine($"[RECEBIDO] Pedido {order.Id} - Cliente: {order.CustomerId} - Prioridade: {order.Priority}");
        
        // Simula tempo de processamento baseado na prioridade
        var processTime = (int)order.Priority * 1000; 
        Console.WriteLine($"[PROCESSANDO] Aguardando {processTime}ms...");
        
        order.StartProcessing(providerId);
        await orderRepository.UpdateAsync(order);
        
        await Task.Delay(processTime);
        
        order.Complete();
        await orderRepository.UpdateAsync(order);
        
        Console.WriteLine($"[CONCLUIDO] Pedido {order.Id} processado com sucesso.");
        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERRO] Falha ao processar pedido {order.Id}: {ex.Message}");
        return false;
    }
});

Console.WriteLine("Aguardando mensagens... Pressione Ctrl+C para sair.");
// Mantem o processo vivo
while (true)
{
    await Task.Delay(1000);
}
