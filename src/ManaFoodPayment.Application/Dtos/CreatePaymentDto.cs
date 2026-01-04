namespace ManaFoodPayment.Application.Dtos;

public class CreatePaymentDto
{
    public Guid OrderId { get; set; }
    public string PaymentId { get; set; } = string.Empty;
    public string QrData { get; set; } = string.Empty;
    public string QrCodeBase64 { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
