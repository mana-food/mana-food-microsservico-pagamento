using System.Net.Http.Headers;
using System.Text.Json;
using ManaFoodPayment.Application.Interfaces;

namespace ManaFoodPayment.Infrastructure.Services;

public class MercadoPagoStatusService : IPaymentStatusService
{
    private readonly HttpClient _httpClient;
    private readonly IPaymentProviderConfig _config;

    public MercadoPagoStatusService(HttpClient httpClient, IPaymentProviderConfig config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<(string status, string orderId)> GetPaymentStatusAsync(string paymentId)
    {
        if (string.IsNullOrWhiteSpace(paymentId) || !long.TryParse(paymentId, out var validatedId))
        {
            throw new ArgumentException("Invalid payment ID format", nameof(paymentId));
        }

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _config.AccessToken);

        var requestUri = new Uri($"https://api.mercadopago.com/v1/payments/{validatedId}");
        var response = await _httpClient.GetAsync(requestUri);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);

        var root = doc.RootElement;

        var status = root.GetProperty("status").GetString()!;

        var orderId = root.GetProperty("external_reference").GetString()!;

        return (status, orderId);
    }
}
