using System;

namespace ManaFoodPayment.Application.Dtos;

public class CreatePaymentRequest
{
    public Guid OrderId { get; set; }
}
