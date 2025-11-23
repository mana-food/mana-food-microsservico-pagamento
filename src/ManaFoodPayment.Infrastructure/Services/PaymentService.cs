namespace ManaFoodPayment.Infrastructure.Services;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ManaFoodPayment.Application.Dtos;
using ManaFoodPayment.Application.Interfaces;
using ManaFoodPayment.Infrastructure.Configurations;
using ManaFoodPayment.Infrastructure.Repositories;
using ManaFoodPayment.Domain.Entities;
using QRCoder;

public class PaymentService : IPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly IPaymentProviderConfig _config;
    private readonly IOrderRepository _orderRepository;

    public PaymentService(HttpClient httpClient, IPaymentProviderConfig config, IOrderRepository orderRepository)
    {
        _httpClient = httpClient;
        _config = config;
        _orderRepository = orderRepository;
    }

    public async Task<CreatePaymentResponse> CreatePaymentAsync(Guid orderId)
    {
        var order = await _orderRepository.GetOrderByIdAsync(orderId);

        if (order == null)
            throw new Exception($"Pedido {orderId} não encontrado.");

        var externalReference = order.Id.ToString();

        var body = new
        {
            external_reference = externalReference,
            title = $"Pedido {externalReference[..8]}",
            description = $"Pedido com valor {order.TotalAmount}",
            notification_url = _config.NotificationUrl,
            total_amount = order.TotalAmount,
            items = Array.Empty<object>(),
            cash_out = new { amount = 0 }
        };

        var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"https://api.mercadopago.com/instore/orders/qr/seller/collectors/{_config.UserId}/pos/{_config.ExternalPosId}/qrs"
        );

        var idempotencyKey = Guid.NewGuid().ToString();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.AccessToken);
        request.Headers.Add("X-Idempotency-Key", idempotencyKey);

        var json = JsonSerializer.Serialize(body, new JsonSerializerOptions { WriteIndented = true });

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

        return new CreatePaymentResponse
        {
            PaymentId = paymentId!,
            QrData = qrData!,
            QrCodeBase64 = qrCodeBase64
        };
    }

    private string GenerateQrCodeBase64(string qrData)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrBytes = qrCode.GetGraphic(20);

        return Convert.ToBase64String(qrBytes);
    }
}
