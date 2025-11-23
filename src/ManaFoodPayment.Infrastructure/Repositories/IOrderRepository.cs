using ManaFoodPayment.Application.Dtos;

namespace ManaFoodPayment.Infrastructure.Repositories;

public interface IOrderRepository
{
    Task<OrderDto?> GetOrderByIdAsync(Guid orderId);
}
