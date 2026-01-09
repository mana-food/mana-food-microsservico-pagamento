namespace ManaFoodPayment.Infrastructure.Services;

using System.Net.Http.Json;
using ManaFoodPayment.Application.Dtos;
using ManaFoodPayment.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class OrderServiceClient : IOrderServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrderServiceClient> _logger;
    private readonly string _baseUrl;

    public OrderServiceClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OrderServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _baseUrl = configuration["OrderService:BaseUrl"]
            ?? throw new InvalidOperationException("OrderService:BaseUrl configuration is required");

        if (!Uri.TryCreate(_baseUrl, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException($"Invalid OrderService BaseUrl: {_baseUrl}", nameof(configuration));
        }

        _httpClient.BaseAddress = uri;
        _httpClient.Timeout = TimeSpan.FromSeconds(30);

        _logger.LogInformation("OrderServiceClient configured with BaseUrl: {BaseUrl}", _baseUrl);
    }

    public async Task<OrderResponseDto?> GetOrderByIdAsync(Guid orderId)
    {
        try
        {
            _logger.LogInformation("Fetching order {OrderId} from Order Service at {BaseUrl}", orderId, _baseUrl);

            var response = await _httpClient.GetAsync($"/api/order/{orderId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Order {OrderId} not found in Order Service", orderId);
                return null;
            }

            response.EnsureSuccessStatusCode();

            var order = await response.Content.ReadFromJsonAsync<OrderResponseDto>();

            _logger.LogInformation("Successfully fetched order {OrderId} with {ItemCount} items",
                orderId, order?.Items?.Count ?? 0);

            return order;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch order {OrderId} from Order Service at {BaseUrl}", orderId, _baseUrl);
            throw new HttpRequestException($"Failed to communicate with Order Service for order {orderId}", ex);
        }
    }

    public async Task UpdateOrderStatusAsync(Guid orderId, string status)
    {
        try
        {
            _logger.LogInformation("Confirming payment for order {OrderId} in Order Service", orderId);

            // Order Service espera apenas um objeto vazio ou mínimo
            // O endpoint é: POST /api/order/{id}/confirm-payment
            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(new { }),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync($"/api/order/{orderId}/confirm-payment", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Payment confirmed successfully for order {OrderId}", orderId);
            }
            else
            {
                _logger.LogWarning("Failed to confirm payment for order {OrderId}. HTTP Status: {StatusCode}",
                    orderId, response.StatusCode);
                response.EnsureSuccessStatusCode();
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error confirming payment for order {OrderId}", orderId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error confirming payment for order {OrderId}", orderId);
            throw;
        }
    }
}