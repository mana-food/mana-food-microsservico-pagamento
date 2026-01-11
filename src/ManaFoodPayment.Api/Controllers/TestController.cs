using Microsoft.AspNetCore.Mvc;
using ManaFoodPayment.Api.Webhooks.MercadoPago;
using ManaFoodPayment.Application.Interfaces;

namespace ManaFoodPayment.Api.Controllers;

/// <summary>
/// Controller para testes e simulações (apenas para desenvolvimento)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IWebhookService _webhookService;
    private readonly ILogger<TestController> _logger;

    public TestController(
        IWebhookService webhookService,
        ILogger<TestController> logger)
    {
        _webhookService = webhookService;
        _logger = logger;
    }

    /// <summary>
    /// Simula um webhook do MercadoPago para testes
    /// </summary>
    /// <remarks>
    /// **ATENÇÃO**: Este endpoint é apenas para TESTES e DESENVOLVIMENTO.
    /// 
    /// Use este endpoint para simular o comportamento do webhook do MercadoPago
    /// sem precisar configurar Ngrok ou fazer pagamentos reais.
    /// 
    /// **Como usar:**
    /// 1. Crie um pedido no Order Service
    /// 2. Gere um pagamento via `/api/payment/create`
    /// 3. Copie o `paymentId` retornado
    /// 4. Use este endpoint para simular a confirmação
    /// 
    /// **Exemplo de payload:**
    /// ```json
    /// {
    ///   "data": {
    ///     "id": "12345678"
    ///   }
    /// }
    /// ```
    /// 
    /// **Comportamento:**
    /// - Funciona igual ao webhook real
    /// - Consulta a API do MercadoPago
    /// - Confirma o pedido se o pagamento estiver aprovado
    /// </remarks>
    /// <param name="payload">Payload simulando o webhook do MercadoPago</param>
    /// <response code="200">Webhook simulado processado com sucesso</response>
    /// <response code="400">Payload inválido</response>
    [HttpPost("simulate-webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SimulateWebhook([FromBody] MercadoPagoWebhookPayload payload)
    {
        if (payload?.Data == null || string.IsNullOrWhiteSpace(payload.Data.Id))
        {
            _logger.LogWarning("Teste: Webhook simulado com payload inválido");
            return BadRequest(new { error = "Invalid payload", message = "Payload deve conter data.id" });
        }

        _logger.LogInformation("🧪 TESTE: Simulando webhook para pagamento: {PaymentId}", payload.Data.Id);

        try
        {
            await _webhookService.ProcessPaymentConfirmationAsync(payload.Data.Id);
            
            return Ok(new 
            { 
                success = true,
                message = $"Webhook simulado com sucesso para pagamento {payload.Data.Id}",
                paymentId = payload.Data.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar webhook simulado");
            return BadRequest(new { error = "Processing error", message = ex.Message });
        }
    }

    /// <summary>
    /// Retorna um exemplo de payload para testar o webhook
    /// </summary>
    /// <remarks>
    /// Use este endpoint para obter um exemplo de como estruturar o payload do webhook.
    /// </remarks>
    /// <response code="200">Exemplo de payload retornado</response>
    [HttpGet("webhook-payload-example")]
    [ProducesResponseType(typeof(MercadoPagoWebhookPayload), StatusCodes.Status200OK)]
    public IActionResult GetWebhookPayloadExample()
    {
        var example = new MercadoPagoWebhookPayload
        {
            Data = new MercadoPagoData
            {
                Id = "12345678"
            }
        };

        return Ok(new
        {
            description = "Exemplo de payload para simular webhook do MercadoPago",
            usage = "POST /api/test/simulate-webhook",
            payload = example,
            instructions = new[]
            {
                "1. Crie um pedido no Order Service",
                "2. Gere um pagamento via POST /api/payment/create",
                "3. Copie o paymentId retornado",
                "4. Use este payload substituindo o ID",
                "5. Envie para POST /api/test/simulate-webhook"
            }
        });
    }

    /// <summary>
    /// Health check do serviço de teste
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            message = "Test endpoints disponíveis"
        });
    }
}
