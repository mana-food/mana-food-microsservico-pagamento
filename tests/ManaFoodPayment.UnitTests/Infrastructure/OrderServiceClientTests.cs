using FluentAssertions;
using ManaFoodPayment.Application.Dtos;
using ManaFoodPayment.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;

namespace ManaFoodPayment.UnitTests.Infrastructure;

public class OrderServiceClientTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<OrderServiceClient>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;

    public OrderServiceClientTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(c => c["OrderService:BaseUrl"]).Returns("http://test-order-service");

        _mockLogger = new Mock<ILogger<OrderServiceClient>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithValidOrder_ReturnsOrderDto()
    {
        var orderId = Guid.NewGuid();
        var orderDto = new OrderResponseDto
        {
            Id = orderId,
            TotalAmount = 100.50m,
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    ProductName = "Produto 1",
                    Quantity = 2,
                    UnitPrice = 50.25m
                }
            }
        };

        var responseContent = JsonSerializer.Serialize(orderDto);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            });

        var client = new OrderServiceClient(_httpClient, _mockConfiguration.Object, _mockLogger.Object);

        var result = await client.GetOrderByIdAsync(orderId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(orderId);
        result.TotalAmount.Should().Be(100.50m);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithNonExistentOrder_ReturnsNull()
    {
        var orderId = Guid.NewGuid();

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        var client = new OrderServiceClient(_httpClient, _mockConfiguration.Object, _mockLogger.Object);

        var result = await client.GetOrderByIdAsync(orderId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetOrderByIdAsync_UsesCorrectEndpoint()
    {
        var orderId = Guid.NewGuid();
        var orderDto = new OrderResponseDto { Id = orderId, TotalAmount = 50 };
        var responseContent = JsonSerializer.Serialize(orderDto);

        HttpRequestMessage? capturedRequest = null;
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            });

        var client = new OrderServiceClient(_httpClient, _mockConfiguration.Object, _mockLogger.Object);

        await client.GetOrderByIdAsync(orderId);

        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri.Should().NotBeNull();
        capturedRequest.RequestUri!.PathAndQuery.Should().Be($"/api/orders/{orderId}");
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithHttpError_ThrowsException()
    {
        var orderId = Guid.NewGuid();

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var client = new OrderServiceClient(_httpClient, _mockConfiguration.Object, _mockLogger.Object);

        await Assert.ThrowsAsync<Exception>(async () =>
            await client.GetOrderByIdAsync(orderId));
    }

    [Fact]
    public void Constructor_UsesDefaultBaseUrl_WhenConfigurationIsEmpty()
    {
        var emptyConfig = new Mock<IConfiguration>();
        emptyConfig.Setup(c => c["OrderService:BaseUrl"]).Returns((string?)null);

        var client = new OrderServiceClient(_httpClient, emptyConfig.Object, _mockLogger.Object);

        _httpClient.BaseAddress.Should().NotBeNull();
        _httpClient.BaseAddress!.ToString().Should().Be("http://order-api-service:8080/");
    }

    [Fact]
    public void Constructor_SetsTimeout()
    {
        var client = new OrderServiceClient(_httpClient, _mockConfiguration.Object, _mockLogger.Object);

        _httpClient.Timeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void Constructor_WithInvalidBaseUrl_ThrowsArgumentException()
    {
        _mockConfiguration.Setup(c => c["OrderService:BaseUrl"]).Returns("invalid-url");

        Assert.Throws<ArgumentException>(() =>
            new OrderServiceClient(_httpClient, _mockConfiguration.Object, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithEmptyBaseUrl_ThrowsArgumentException()
    {
        _mockConfiguration.Setup(c => c["OrderService:BaseUrl"]).Returns(string.Empty);

        Assert.Throws<ArgumentException>(() =>
            new OrderServiceClient(_httpClient, _mockConfiguration.Object, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullBaseUrl_UsesDefaultValue()
    {
        _mockConfiguration.Setup(c => c["OrderService:BaseUrl"]).Returns((string?)null);

        var client = new OrderServiceClient(_httpClient, _mockConfiguration.Object, _mockLogger.Object);

        _httpClient.BaseAddress.Should().NotBeNull();
        _httpClient.BaseAddress!.ToString().Should().Be("http://order-api-service:8080/");
    }
}
