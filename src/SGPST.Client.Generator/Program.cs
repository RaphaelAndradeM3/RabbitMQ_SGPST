using System.Text;
using System.Text.Json;
using SGPST.Application.DTOs;
using SGPST.Domain.Entities;

Console.WriteLine("Iniciando Gerador de Pedidos Simulados...");
Console.WriteLine("Aguardando 5 segundos para garantir que a API esteja online...");
await Task.Delay(5000);

using var client = new HttpClient();
client.BaseAddress = new Uri("http://localhost:5000/"); // Ajuste para a URL real da sua API

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
        var json = JsonSerializer.Serialize(createOrderDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        Console.WriteLine($"[GERANDO] Solicitando pedido via API - Cliente: {customerId} - Prioridade: {prioridade}");
        
        var response = await client.PostAsync("api/Orders", content);
        
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[SUCESSO] Pedido enviado e persistido via API.");
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[FALHA] API retornou erro: {response.StatusCode} - {error}");
        }
        
        // Espera entre 3 e 7 segundos para gerar o proximo
        await Task.Delay(random.Next(3000, 7000));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERRO] Falha ao comunicar com a API: {ex.Message}");
        await Task.Delay(5000);
    }
}
