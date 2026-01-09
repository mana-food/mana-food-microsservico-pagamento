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
        capturedRequest.RequestUri!.PathAndQuery.Should().Be($"/api/order/{orderId}");
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithHttpError_ThrowsHttpRequestException()
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

        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await client.GetOrderByIdAsync(orderId));
        
        exception.Message.Should().Contain("Failed to communicate with Order Service");
    }    [Fact]
    public void Constructor_UsesDefaultBaseUrl_WhenConfigurationIsEmpty()
    {
        var emptyConfig = new Mock<IConfiguration>();
        emptyConfig.Setup(c => c["OrderService:BaseUrl"]).Returns((string?)null);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new OrderServiceClient(_httpClient, emptyConfig.Object, _mockLogger.Object));

        exception.Message.Should().Be("OrderService:BaseUrl configuration is required");
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
    public void Constructor_WithNullBaseUrl_ThrowsInvalidOperationException()
    {
        _mockConfiguration.Setup(c => c["OrderService:BaseUrl"]).Returns((string?)null);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new OrderServiceClient(_httpClient, _mockConfiguration.Object, _mockLogger.Object));

        exception.Message.Should().Be("OrderService:BaseUrl configuration is required");
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WithSuccessfulResponse_CompletesSuccessfully()
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
                StatusCode = HttpStatusCode.OK
            });

        var client = new OrderServiceClient(_httpClient, _mockConfiguration.Object, _mockLogger.Object);

        var act = async () => await client.UpdateOrderStatusAsync(orderId, "Paid");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_UsesCorrectEndpoint()
    {
        var orderId = Guid.NewGuid();

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
                StatusCode = HttpStatusCode.OK
            });

        var client = new OrderServiceClient(_httpClient, _mockConfiguration.Object, _mockLogger.Object);

        await client.UpdateOrderStatusAsync(orderId, "Paid");

        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Post);
        capturedRequest.RequestUri.Should().NotBeNull();
        capturedRequest.RequestUri!.PathAndQuery.Should().Be($"/api/order/{orderId}/confirm-payment");
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WithFailedResponse_ThrowsHttpRequestException()
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
                StatusCode = HttpStatusCode.BadRequest
            });

        var client = new OrderServiceClient(_httpClient, _mockConfiguration.Object, _mockLogger.Object);

        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await client.UpdateOrderStatusAsync(orderId, "Paid"));

        exception.Message.Should().Contain("Failed to confirm payment for order");
        exception.InnerException.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WithHttpRequestException_WrapsException()
    {
        var orderId = Guid.NewGuid();

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var client = new OrderServiceClient(_httpClient, _mockConfiguration.Object, _mockLogger.Object);

        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await client.UpdateOrderStatusAsync(orderId, "Paid"));

        exception.Message.Should().Contain("Failed to confirm payment for order");
        exception.InnerException.Should().NotBeNull();
        exception.InnerException!.Message.Should().Contain("Connection refused");
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WithGenericException_WrapsAsInvalidOperationException()
    {
        var orderId = Guid.NewGuid();

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        var client = new OrderServiceClient(_httpClient, _mockConfiguration.Object, _mockLogger.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await client.UpdateOrderStatusAsync(orderId, "Paid"));

        exception.Message.Should().Contain("Unexpected error confirming payment for order");
        exception.InnerException.Should().NotBeNull();
        exception.InnerException!.Message.Should().Contain("Unexpected error");
    }
}
