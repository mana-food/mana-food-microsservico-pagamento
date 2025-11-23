using ManaFoodPayment.Domain.Entities;
using ManaFoodPayment.Application.Dtos;

namespace ManaFoodPayment.Infrastructure.Repositories;

public class InMemoryOrderRepository : IOrderRepository
{
    private static readonly List<Order> _orders = new()
    {
        new Order { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), TotalAmount = 100.0m }
    };

    public Task<Order?> GetByIdAsync(Guid id)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        return Task.FromResult(order);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId)
    {
        var order = _orders.FirstOrDefault(o => o.Id == orderId);
        if (order == null) return null;
        return await Task.FromResult(new OrderDto
        {
            Id = order.Id,
            TotalAmount = order.TotalAmount
        });
    }
}
