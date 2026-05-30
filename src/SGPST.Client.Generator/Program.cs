using SGPST.Domain.Entities;
using SGPST.Domain.Interfaces;
using SGPST.Infrastructure.Messaging;

Console.WriteLine("Iniciando Gerador de Pedidos Simulados...");

var broker = new RabbitMqBroker("localhost");
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

        var order = Order.Create(customerId, descricao, prioridade);
        
        Console.WriteLine($"[GERANDO] Pedido {order.Id} - Cliente: {customerId} - Prioridade: {prioridade}");
        
        await broker.PublishOrderAsync(order);
        
        // Espera entre 2 e 5 segundos para gerar o proximo
        await Task.Delay(random.Next(2000, 5000));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERRO] Falha ao gerar pedido: {ex.Message}");
        await Task.Delay(5000);
    }
}
