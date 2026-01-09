using ManaFoodPayment.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ManaFoodPayment.Infrastructure.Services;

public class WebhookService : IWebhookService
{
    private readonly IPaymentStatusService _paymentStatusService;
    private readonly IOrderServiceClient _orderServiceClient;
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(
        IPaymentStatusService paymentStatusService,
        IOrderServiceClient orderServiceClient,
        ILogger<WebhookService> logger)
    {
        _paymentStatusService = paymentStatusService;
        _orderServiceClient = orderServiceClient;
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
            
            // ✅ ATUALIZA O STATUS DO PEDIDO (Requisito: "atualizando o status do pedido")
            try
            {
                await _orderServiceClient.UpdateOrderStatusAsync(orderGuid, "RECEIVED");
                _logger.LogInformation("Payment {PaymentId} approved and Order {OrderId} status updated to RECEIVED successfully", 
                    paymentId, orderGuid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update order {OrderId} status after payment approval. Payment was approved but order status may not have been updated.", orderGuid);
                // Não propaga a exceção para não quebrar o webhook do MercadoPago
                // Em produção, implementar fila de retry
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment confirmation: {PaymentId}", paymentId);
        }
    }
}
