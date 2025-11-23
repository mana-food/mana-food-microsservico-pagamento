using MediatR;
using ManaFoodPayment.Application.Dtos;

namespace ManaFoodPayment.Application.UseCases.CreatePayment;

public class CreatePaymentCommand : IRequest<CreatePaymentResponse>
{
    public Guid OrderId { get; set; }
}
