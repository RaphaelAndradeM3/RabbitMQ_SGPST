using SGPST.Domain.Interfaces;
using SGPST.Infrastructure.Messaging;

Console.WriteLine("Iniciando Worker de Processamento de Pedidos (Via API)...");

var providerId = $"Prestador-{Guid.NewGuid().ToString().Substring(0, 8)}";
var broker = new RabbitMqBroker("localhost");
using var httpClient = new HttpClient();
httpClient.BaseAddress = new Uri("http://localhost:5042/"); // URL da API

Console.WriteLine($"Conectado como {providerId}");

broker.SubscribeToOrders(async (order) =>
{
    try
    {
        Console.WriteLine($"[RECEBIDO] Pedido {order.Id} - Cliente: {order.CustomerId} - Prioridade: {order.Priority}");
        
        // 1. Notifica a API que iniciou o processamento
        var startResponse = await httpClient.PatchAsync($"api/Orders/{order.Id}/start-processing?providerId={providerId}", null);
        
        if (!startResponse.IsSuccessStatusCode)
        {
            Console.WriteLine($"[ERRO] Falha ao iniciar na API: {startResponse.StatusCode}");
            return false;
        }

        // Simula tempo de processamento baseado na prioridade
        var processTime = (int)order.Priority * 1000; 
        Console.WriteLine($"[PROCESSANDO] Aguardando {processTime}ms...");
        await Task.Delay(processTime);
        
        // 2. Notifica a API que concluiu o processamento
        var completeResponse = await httpClient.PatchAsync($"api/Orders/{order.Id}/complete", null);

        if (completeResponse.IsSuccessStatusCode)
        {
            Console.WriteLine($"[CONCLUIDO] Pedido {order.Id} processado com sucesso.");
            return true;
        }
        
        Console.WriteLine($"[ERRO] Falha ao concluir na API: {completeResponse.StatusCode}");
        return false;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERRO] Falha fatal no worker: {ex.Message}");
        return false;
    }
});

Console.WriteLine("Aguardando mensagens... Pressione Ctrl+C para sair.");
while (true) await Task.Delay(1000);
