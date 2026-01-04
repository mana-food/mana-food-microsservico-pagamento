namespace ManaFoodPayment.Application.Dtos;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string PaymentId { get; set; } = string.Empty;
    public string QrData { get; set; } = string.Empty;
    public string QrCodeBase64 { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
}
