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
    public Guid Id { get; private set; }
    public string CustomerId { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public OrderPriority Priority { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? ProviderId { get; private set; }

    // Construtor privado com atributo para o Serializador JSON
    [JsonConstructor]
    private Order(Guid id, string customerId, string description, OrderPriority priority, OrderStatus status, DateTime createdAt, DateTime? processedAt, string? providerId)
    {
        Id = id;
        CustomerId = customerId;
        Description = description;
        Priority = priority;
        Status = status;
        CreatedAt = createdAt;
        ProcessedAt = processedAt;
        ProviderId = providerId;
    }

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
