using Microsoft.AspNetCore.Mvc;
using ManaFoodPayment.Application.Interfaces;

namespace ManaFoodPayment.Api.Webhooks.MercadoPago;

/// <summary>
/// Webhook para receber notificações de pagamento do MercadoPago
/// </summary>
[ApiController]
[Route("api/webhooks/mercadopago")]
[Produces("application/json")]
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

    /// <summary>
    /// Recebe confirmação de pagamento do MercadoPago (Webhook)
    /// </summary>
    /// <remarks>
    /// Este endpoint é chamado automaticamente pelo MercadoPago quando um pagamento é processado.
    /// 
    /// **ATENÇÃO**: Este é um webhook público que não requer autenticação.
    /// 
    /// **Fluxo:**
    /// 1. MercadoPago processa o pagamento
    /// 2. MercadoPago chama este endpoint com o ID do pagamento
    /// 3. Sistema consulta status na API do MercadoPago
    /// 4. Se aprovado, confirma o pedido no Order Service
    /// 
    /// **Para testes**: Use o endpoint `/api/test/simulate-webhook` para simular um webhook
    /// </remarks>
    /// <param name="payload">Payload do webhook contendo o ID do pagamento</param>
    /// <response code="200">Webhook processado com sucesso</response>
    /// <response code="400">Payload inválido ou dados ausentes</response>
    [HttpPost("payment-confirmation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
