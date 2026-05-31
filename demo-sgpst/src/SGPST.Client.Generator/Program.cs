using System.Net.Http.Json;
using Serilog;
using SGPST.Application.DTOs;
using SGPST.Domain.Entities;
using SGPST.Infrastructure.Data;

SerilogConfig.Configure("Generator");

try
{
    Log.Information("Iniciando Gerador de Pedidos Simulados (Via API)...");

    using var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri("http://localhost:5042/");

    var random = new Random();
    string[] descricoes = { 
        "Problema no acesso ao email", 
        "Impressora nao funciona", 
        "Sistema lento", 
        "Recuperacao de senha", 
        "Instalacao de software" 
    };

    while (true)
    {
        try
        {
            var customerId = $"Cliente-{random.Next(1, 100)}";
            var descricao = descricoes[random.Next(descricoes.Length)];
            var prioridade = (OrderPriority)random.Next(1, 5);

            var createOrderDto = new CreateOrderDto(customerId, descricao, prioridade);

            Log.Information("[GERANDO] Solicitando pedido via API - Cliente: {CustomerId} - Prioridade: {Priority}", 
                customerId, prioridade);
            
            var response = await httpClient.PostAsJsonAsync("api/Orders", createOrderDto);
            
            if (response.IsSuccessStatusCode)
            {
                Log.Information("[SUCESSO] Pedido enviado e persistido via API.");
            }
            else
            {
                Log.Warning("[FALHA] API retornou erro: {StatusCode}", response.StatusCode);
            }
            
            await Task.Delay(random.Next(3000, 7000));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Falha ao comunicar com a API");
            await Task.Delay(5000);
        }
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "Generator falhou!");
}
finally
{
    Log.CloseAndFlush();
}
