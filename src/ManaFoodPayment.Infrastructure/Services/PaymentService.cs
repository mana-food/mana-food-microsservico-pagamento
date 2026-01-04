namespace ManaFoodPayment.Infrastructure.Services;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ManaFoodPayment.Application.Dtos;
using ManaFoodPayment.Application.Interfaces;
using ManaFoodPayment.Infrastructure.Configurations;
using QRCoder;
using Microsoft.Extensions.Logging;

public class PaymentService : IPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly IPaymentProviderConfig _config;
    private readonly IOrderServiceClient _orderServiceClient;
    private readonly ILogger<PaymentService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public PaymentService(
        HttpClient httpClient, 
        IPaymentProviderConfig config, 
        IOrderServiceClient orderServiceClient,
        ILogger<PaymentService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _orderServiceClient = orderServiceClient;
        _logger = logger;
    }

    public async Task<CreatePaymentResponseDto> CreatePaymentAsync(Guid orderId)
    {
        _logger.LogInformation("Creating payment for order {OrderId}", orderId);
        
        // BUSCAR PEDIDO VIA REST DO ORDER SERVICE
        var orderDto = await _orderServiceClient.GetOrderByIdAsync(orderId);

        if (orderDto == null)
        {
            _logger.LogWarning("Order {OrderId} not found", orderId);
            throw new Exception($"Pedido {orderId} não encontrado no Order Service.");
        }

        _logger.LogInformation("Order {OrderId} found with {ItemCount} items, total: {TotalAmount}", 
            orderId, orderDto.Items.Count, orderDto.TotalAmount);

        var externalReference = orderDto.Id.ToString();

        var items = orderDto.Items.Select(item => new
        {
            sku_number = item.ProductId.ToString(),
            category = "marketplace",
            title = item.ProductName,
            description = item.ProductDescription ?? "Produto sem descrição",
            unit_price = item.UnitPrice,
            quantity = item.Quantity,
            unit_measure = "unit",
            total_amount = item.TotalAmount
        }).ToArray();

        var body = new
        {
            external_reference = externalReference,
            title = $"Pedido {externalReference[..8]}",
            description = $"Pedido com {items.Length} item(ns)",
            notification_url = _config.NotificationUrl,
            total_amount = orderDto.TotalAmount,
            items = items,
            cash_out = new { amount = 0 }
        };

        var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"https://api.mercadopago.com/instore/orders/qr/seller/collectors/{_config.UserId}/pos/{_config.ExternalPosId}/qrs"
        );

        var idempotencyKey = Guid.NewGuid().ToString();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.AccessToken);
        request.Headers.Add("X-Idempotency-Key", idempotencyKey);

        var json = JsonSerializer.Serialize(body, JsonOptions);

        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Erro de chamada: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseContent);
        var root = doc.RootElement;

        var qrData = root.GetProperty("qr_data").GetString();
        var paymentId = root.GetProperty("in_store_order_id").GetString();

        string qrCodeBase64 = GenerateQrCodeBase64(qrData!);

        return new CreatePaymentResponseDto
        {
            PaymentId = paymentId!,
            QrData = qrData!,
            QrCodeBase64 = qrCodeBase64
        };
    }

    private static string GenerateQrCodeBase64(string qrData)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrBytes = qrCode.GetGraphic(20);

        return Convert.ToBase64String(qrBytes);
    }
}
