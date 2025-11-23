namespace ManaFoodPayment.Infrastructure.Configurations;

using ManaFoodPayment.Application.Interfaces;

public class PaymentProviderConfig : IPaymentProviderConfig
{
    public string AccessToken { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ExternalPosId { get; set; } = string.Empty;
    public string NotificationUrl { get; set; } = string.Empty;
}
