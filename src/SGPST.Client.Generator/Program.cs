using SGPST.Application.Services;
using SGPST.Domain.Entities;
using SGPST.Infrastructure.Data;
using SGPST.Infrastructure.Messaging;
using SGPST.Infrastructure.Repositories;

Console.WriteLine("Iniciando Gerador de Pedidos Simulados (Direto no Servico)...");

// Setup das dependencias (mesmo que a API, usando o banco unificado)
var dbFactory = new SqliteConnectionFactory();
var broker = new RabbitMqBroker("localhost");
var orderRepo = new OrderRepository(dbFactory);
var userRepo = new UserRepository(dbFactory);

// O Servico encapsula a persistencia no Banco E a publicacao no RabbitMQ
var orderService = new OrderService(orderRepo, broker);

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

        var createOrderDto = new SGPST.Application.DTOs.CreateOrderDto(customerId, descricao, prioridade);

        Console.WriteLine($"[GERANDO] Pedido via Servico - Cliente: {customerId} - Prioridade: {prioridade}");
        
        var result = await orderService.SubmitOrderAsync(createOrderDto);
        
        if (result.Success)
        {
            Console.WriteLine($"[SUCESSO] Pedido {result.Data!.Id} persistido e enviado para a fila.");
        }
        else
        {
            Console.WriteLine($"[FALHA] Erro no servico: {result.Message}");
        }
        
        // Espera entre 3 e 7 segundos para gerar o proximo
        await Task.Delay(random.Next(3000, 7000));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERRO] Falha fatal no gerador: {ex.Message}");
        await Task.Delay(5000);
    }
}
