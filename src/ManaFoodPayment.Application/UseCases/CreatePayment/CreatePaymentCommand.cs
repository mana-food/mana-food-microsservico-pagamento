using MediatR;
using ManaFoodPayment.Application.Dtos;

namespace ManaFoodPayment.Application.UseCases.CreatePayment;

public class CreatePaymentCommand : IRequest<CreatePaymentResponseDto>
{
    public Guid OrderId { get; set; }
}
