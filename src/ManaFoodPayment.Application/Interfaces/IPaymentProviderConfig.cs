namespace ManaFoodPayment.Application.Interfaces;

public interface IPaymentProviderConfig
{
    string AccessToken { get; }
    string UserId { get; }
    string ExternalPosId { get; }
    string NotificationUrl { get; }
}
