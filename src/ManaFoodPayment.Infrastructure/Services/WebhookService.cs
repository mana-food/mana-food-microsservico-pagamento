using ManaFoodPayment.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ManaFoodPayment.Infrastructure.Services;

/// <summary>
/// Serviço de processamento de webhooks - Padrão ConfirmPaymentHandler contido no monolito
/// </summary>
public class WebhookService : IWebhookService
{
    private readonly IPaymentStatusService _paymentStatusService;
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(
        IPaymentStatusService paymentStatusService,
        ILogger<WebhookService> logger)
    {
        _paymentStatusService = paymentStatusService;
        _logger = logger;
    }

    public async Task ProcessPaymentConfirmationAsync(string paymentId)
    {
        try
        {
            _logger.LogInformation("Processing payment confirmation: {PaymentId}", paymentId);

            var (status, orderId) = await _paymentStatusService.GetPaymentStatusAsync(paymentId);

            if (status != "approved")
            {
                _logger.LogWarning("Payment {PaymentId} not approved. Status: {Status}", paymentId, status);
                return;
            }

            var orderGuid = Guid.Parse(orderId);
            
            _logger.LogInformation("Payment {PaymentId} approved for Order {OrderId}. Order status should change to RECEIVED.", 
                paymentId, orderGuid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment confirmation: {PaymentId}", paymentId);
        }
    }
}
