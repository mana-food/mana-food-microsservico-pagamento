namespace ManaFoodPayment.Infrastructure.Services;

using System.Net.Http.Json;
using ManaFoodPayment.Application.Dtos;
using ManaFoodPayment.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

/// <summary>
/// Cliente HTTP para comunicação REST com o Order Service
/// </summary>
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
        _baseUrl = configuration["OrderService:BaseUrl"] ?? "http://order-api-service:8080";
        
        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        
        _logger.LogInformation("OrderServiceClient configured with BaseUrl: {BaseUrl}", _baseUrl);
    }

    public async Task<OrderResponseDto?> GetOrderByIdAsync(Guid orderId)
    {
        try
        {
            _logger.LogInformation("Fetching order {OrderId} from Order Service at {BaseUrl}", orderId, _baseUrl);

            var response = await _httpClient.GetAsync($"/api/orders/{orderId}");

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
            _logger.LogError(ex, "HTTP error calling Order Service for order {OrderId}: {Message}", orderId, ex.Message);
            throw new Exception($"Erro ao buscar pedido no Order Service: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching order {OrderId}: {Message}", orderId, ex.Message);
            throw;
        }
    }
}
