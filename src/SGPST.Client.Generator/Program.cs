using System.Net.Http.Json;
using SGPST.Application.DTOs;
using SGPST.Domain.Entities;

Console.WriteLine("Iniciando Gerador de Pedidos Simulados (Via API)...");

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

        Console.WriteLine($"[GERANDO] Solicitando pedido via API - Cliente: {customerId} - Prioridade: {prioridade}");
        
        var response = await httpClient.PostAsJsonAsync("api/Orders", createOrderDto);
        
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[SUCESSO] Pedido enviado e persistido via API.");
        }
        else
        {
            Console.WriteLine($"[FALHA] API retornou erro: {response.StatusCode}");
        }
        
        await Task.Delay(random.Next(3000, 7000));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERRO] Falha ao comunicar com a API: {ex.Message}");
        await Task.Delay(5000);
    }
}
