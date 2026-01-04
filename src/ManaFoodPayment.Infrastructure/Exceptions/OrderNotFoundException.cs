namespace ManaFoodPayment.Infrastructure.Exceptions;

public class OrderNotFoundException : Exception
{
    public OrderNotFoundException(Guid orderId) 
        : base($"Pedido {orderId} não encontrado no Order Service.")
    {
        OrderId = orderId;
    }

    public Guid OrderId { get; }
}
