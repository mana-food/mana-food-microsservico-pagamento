using System;

namespace ManaFoodPayment.Domain.Entities;
public class Order
{
    public Guid Id { get; set; }
    public decimal TotalAmount { get; set; }
}
