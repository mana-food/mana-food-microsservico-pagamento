using ManaFoodPayment.Application.Dtos;

namespace ManaFoodPayment.Application.Interfaces;

public interface IPaymentService
{
    Task<CreatePaymentResponseDto> CreatePaymentAsync(Guid orderId);
}
