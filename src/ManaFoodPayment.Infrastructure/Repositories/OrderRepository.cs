namespace ManaFoodPayment.Infrastructure.Repositories;

using System.Text.Json;
using ManaFoodPayment.Application.Dtos;

public class OrderRepository : IOrderRepository
{
    private readonly HttpClient _httpClient;

    public OrderRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId)
    {
        var response = await _httpClient.GetAsync($"http://pedido-service/api/orders/{orderId}");
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OrderDto>(json);
    }
}
