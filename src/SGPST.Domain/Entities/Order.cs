using System.Text.Json.Serialization;

namespace SGPST.Domain.Entities;

public enum OrderPriority
{
    Baixa = 1,
    Media = 2,
    Alta = 3,
    Urgente = 4
}

public enum OrderStatus
{
    Pendente = 1,
    EmProcessamento = 2,
    Concluido = 3,
    Cancelado = 4
}

public class Order
{
    public Guid Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public OrderPriority Priority { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProviderId { get; set; }

    // Construtor vazio publico para o Dapper (Garante a materializacao)
    public Order() { }

    // Construtor para o Factory Method (novo pedido)
    private Order(Guid id, string customerId, string description, OrderPriority priority)
    {
        Id = id;
        CustomerId = customerId;
        Description = description;
        Priority = priority;
        Status = OrderStatus.Pendente;
        CreatedAt = DateTime.UtcNow;
    }

    // Factory Method para criacao de novos pedidos
    public static Order Create(string customerId, string description, OrderPriority priority)
    {
        return new Order(Guid.NewGuid(), customerId, description, priority);
    }

    // Metodo para atualizar status para processamento
    public void StartProcessing(string providerId)
    {
        Status = OrderStatus.EmProcessamento;
        ProviderId = providerId;
    }

    // Metodo para finalizar o pedido
    public void Complete()
    {
        Status = OrderStatus.Concluido;
        ProcessedAt = DateTime.UtcNow;
    }
}
