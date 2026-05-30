using SGPST.Domain.Entities;

namespace SGPST.Domain.Interfaces;

public interface IMessageBroker
{
    // Metodo para publicar uma mensagem (Pedido) na fila
    Task PublishOrderAsync(Order order);
    
    // Metodo para subscrever ao processamento de pedidos
    void SubscribeToOrders(Func<Order, Task<bool>> onMessageReceived);
}
