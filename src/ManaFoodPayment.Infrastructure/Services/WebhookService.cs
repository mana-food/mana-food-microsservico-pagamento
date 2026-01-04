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
            _logger.LogInformation("Processando confirmação de pagamento: {PaymentId}", paymentId);

            var (status, orderId) = await _paymentStatusService.GetPaymentStatusAsync(paymentId);

            _logger.LogInformation("Pagamento {PaymentId} consultado: Status={Status}, OrderId={OrderId}", 
                paymentId, status, orderId);

            if (status != "approved")
            {
                _logger.LogWarning("Pagamento {PaymentId} não aprovado. Status: {Status}", paymentId, status);
                return;
            }

            var orderGuid = Guid.Parse(orderId);
            
            _logger.LogInformation("Pagamento aprovado para Order {OrderId}. Pedido deve mudar status para RECEIVED.", orderGuid);
            
            _logger.LogInformation("Confirmação de pagamento processada com sucesso: {PaymentId}", paymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar confirmação de pagamento: {PaymentId}", paymentId);
        }
    }
}
