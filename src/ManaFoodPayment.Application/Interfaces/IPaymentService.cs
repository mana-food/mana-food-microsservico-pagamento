using ManaFoodPayment.Application.Dtos;

namespace ManaFoodPayment.Application.Interfaces;

public interface IPaymentService
{
    Task<CreatePaymentResponse> CreatePaymentAsync(Guid orderId);
}
