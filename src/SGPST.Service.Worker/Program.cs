using Serilog;
using SGPST.Domain.Interfaces;
using SGPST.Infrastructure.Data;
using SGPST.Infrastructure.Messaging;

SerilogConfig.Configure("Worker");

try
{
    Log.Information("Iniciando Worker de Processamento de Pedidos (Via API)...");

    var providerId = $"Prestador-{Guid.NewGuid().ToString().Substring(0, 8)}";
    var broker = new RabbitMqBroker("localhost");
    using var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri("http://localhost:5042/"); // URL da API

    Log.Information("Conectado como {ProviderId}", providerId);

    broker.SubscribeToOrders(async (order) =>
    {
        try
        {
            Log.Information("[RECEBIDO] Pedido {OrderId} - Cliente: {CustomerId} - Prioridade: {Priority}", 
                order.Id, order.CustomerId, order.Priority);
            
            // 1. Notifica a API que iniciou o processamento
            var startResponse = await httpClient.PatchAsync($"api/Orders/{order.Id}/start-processing?providerId={providerId}", null);
            
            if (!startResponse.IsSuccessStatusCode)
            {
                Log.Error("[ERRO] Falha ao iniciar na API: {StatusCode}", startResponse.StatusCode);
                return false;
            }

            // Simula tempo de processamento baseado na prioridade
            var processTime = (int)order.Priority * 1000; 
            Log.Information("[PROCESSANDO] Aguardando {ProcessTime}ms...", processTime);
            await Task.Delay(processTime);
            
            // 2. Notifica a API que concluiu o processamento
            var completeResponse = await httpClient.PatchAsync($"api/Orders/{order.Id}/complete", null);

            if (completeResponse.IsSuccessStatusCode)
            {
                Log.Information("[CONCLUIDO] Pedido {OrderId} processado com sucesso.", order.Id);
                return true;
            }
            
            Log.Error("[ERRO] Falha ao concluir na API: {StatusCode}", completeResponse.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Falha fatal no worker ao processar pedido {OrderId}", order.Id);
            return false;
        }
    });

    Log.Information("Aguardando mensagens... Pressione Ctrl+C para sair.");
    while (true) await Task.Delay(1000);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker falhou!");
}
finally
{
    Log.CloseAndFlush();
}
