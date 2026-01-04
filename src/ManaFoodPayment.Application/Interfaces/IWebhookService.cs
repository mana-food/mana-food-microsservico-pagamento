namespace ManaFoodPayment.Application.Interfaces;

public interface IWebhookService
{
    Task ProcessPaymentConfirmationAsync(string paymentId);
}
