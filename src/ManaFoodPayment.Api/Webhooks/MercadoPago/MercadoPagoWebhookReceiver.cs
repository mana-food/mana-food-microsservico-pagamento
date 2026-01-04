using Microsoft.AspNetCore.Mvc;
using ManaFoodPayment.Application.Interfaces;

namespace ManaFoodPayment.Api.Webhooks.MercadoPago;

[ApiController]
[Route("api/webhooks/mercadopago")]
public class MercadoPagoWebhookReceiver : ControllerBase
{
    private readonly IWebhookService _webhookService;
    private readonly ILogger<MercadoPagoWebhookReceiver> _logger;

    public MercadoPagoWebhookReceiver(
        IWebhookService webhookService,
        ILogger<MercadoPagoWebhookReceiver> logger)
    {
        _webhookService = webhookService;
        _logger = logger;
    }

    [HttpPost("payment-confirmation")]
    public async Task<IActionResult> ReceivePaymentConfirmation([FromBody] MercadoPagoWebhookPayload payload)
    {
        if (payload?.Data == null || string.IsNullOrWhiteSpace(payload.Data.Id))
        {
            _logger.LogWarning("Webhook recebido com payload inválido");
            return BadRequest("Invalid payload");
        }

        _logger.LogInformation("Webhook recebido para pagamento: {PaymentId}", payload.Data.Id);

        await _webhookService.ProcessPaymentConfirmationAsync(payload.Data.Id);

        return Ok();
    }
}
