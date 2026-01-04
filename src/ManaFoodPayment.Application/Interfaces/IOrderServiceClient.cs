namespace ManaFoodPayment.Application.Interfaces;

using ManaFoodPayment.Application.Dtos;

public interface IOrderServiceClient
{
    Task<OrderResponseDto?> GetOrderByIdAsync(Guid orderId);
}
