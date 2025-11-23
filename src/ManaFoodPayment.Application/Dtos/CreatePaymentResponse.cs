namespace ManaFoodPayment.Application.Dtos;

public class CreatePaymentResponse
{
    public string PaymentId { get; set; } = string.Empty;
    public string QrData { get; set; } = string.Empty;
    public string QrCodeBase64 { get; set; } = string.Empty;
}
