using FluentAssertions;
using ManaFoodPayment.Application.Dtos;
using ManaFoodPayment.Application.Interfaces;
using ManaFoodPayment.Infrastructure.Configurations;
using ManaFoodPayment.Infrastructure.Services;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ManaFoodPayment.UnitTests.Application;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentProviderConfig> _mockConfig;
    private readonly Mock<IOrderServiceClient> _mockOrderServiceClient;
    private readonly Mock<ILogger<PaymentService>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private readonly HttpClient _httpClient;
    private readonly PaymentService _service;

    public PaymentServiceTests()
    {
        _mockConfig = new Mock<IPaymentProviderConfig>();
        _mockOrderServiceClient = new Mock<IOrderServiceClient>();
        _mockLogger = new Mock<ILogger<PaymentService>>();
        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpHandler.Object);

        _mockConfig.Setup(c => c.AccessToken).Returns("test-access-token");
        _mockConfig.Setup(c => c.UserId).Returns("123456789");
        _mockConfig.Setup(c => c.ExternalPosId).Returns("POS001");
        _mockConfig.Setup(c => c.NotificationUrl).Returns("https://test.com/webhook");

        _service = new PaymentService(
            _httpClient, 
            _mockConfig.Object, 
            _mockOrderServiceClient.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task CreatePaymentAsync_WithValidOrder_ReturnsPaymentResponse()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = new OrderResponseDto
        {
            Id = orderId,
            TotalAmount = 100.00m,
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductName = "X-Burger",
                    ProductDescription = "Delicioso hambúrguer",
                    Quantity = 2,
                    UnitPrice = 50.00m,
                    TotalAmount = 100.00m
                }
            }
        };

        var mercadoPagoResponse = new
        {
            qr_data = "00020126580014br.gov.bcb.pix...",
            in_store_order_id = "MP123456"
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(mercadoPagoResponse))
        };

_mockOrderServiceClient
            .Setup(c => c.GetOrderByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _service.CreatePaymentAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.PaymentId.Should().Be("MP123456");
        result.QrData.Should().Be("00020126580014br.gov.bcb.pix...");
        result.QrCodeBase64.Should().NotBeNullOrEmpty();
        _mockOrderServiceClient.Verify(c => c.GetOrderByIdAsync(orderId), Times.Once);
    }

    [Fact]
    public async Task CreatePaymentAsync_WithNonExistentOrder_ThrowsException()
    {
        // Arrange
        var orderId = Guid.NewGuid();

_mockOrderServiceClient
            .Setup(c => c.GetOrderByIdAsync(orderId))
            .ReturnsAsync((OrderResponseDto?)null);

        // Act
        Func<Task> act = async () => await _service.CreatePaymentAsync(orderId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*Pedido {orderId} não encontrado no Order Service.*");
    }

    [Fact]
    public async Task CreatePaymentAsync_WithMercadoPagoError_ThrowsException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = new OrderResponseDto
        {
            Id = orderId,
            TotalAmount = 50.00m,
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductName = "Batata Frita",
                    Quantity = 1,
                    UnitPrice = 50.00m,
                    TotalAmount = 50.00m
                }
            }
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);

_mockOrderServiceClient
            .Setup(c => c.GetOrderByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        Func<Task> act = async () => await _service.CreatePaymentAsync(orderId);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("*Erro ao criar pagamento no MercadoPago: BadRequest*");
    }

    [Fact]
    public async Task CreatePaymentAsync_SetsCorrectHeaders()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = new OrderResponseDto
        {
            Id = orderId,
            TotalAmount = 25.00m,
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductName = "Refrigerante",
                    Quantity = 1,
                    UnitPrice = 25.00m,
                    TotalAmount = 25.00m
                }
            }
        };

        var mercadoPagoResponse = new
        {
            qr_data = "pix-data",
            in_store_order_id = "MP999"
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(mercadoPagoResponse))
        };

        HttpRequestMessage? capturedRequest = null;

_mockOrderServiceClient
            .Setup(c => c.GetOrderByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(responseMessage);

        // Act
        await _service.CreatePaymentAsync(orderId);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.Authorization!.Scheme.Should().Be("Bearer");
        capturedRequest.Headers.Authorization.Parameter.Should().Be("test-access-token");
        capturedRequest.Headers.Contains("X-Idempotency-Key").Should().BeTrue();
    }

    [Fact]
    public async Task CreatePaymentAsync_WithMultipleItems_CreatesCorrectPayload()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = new OrderResponseDto
        {
            Id = orderId,
            TotalAmount = 250.00m,
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductName = "Item 1",
                    Quantity = 2,
                    UnitPrice = 50.00m,
                    TotalAmount = 100.00m
                },
                new OrderItemDto
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductName = "Item 2",
                    Quantity = 3,
                    UnitPrice = 50.00m,
                    TotalAmount = 150.00m
                }
            }
        };

        var mercadoPagoResponse = new
        {
            qr_data = "pix-data-multi",
            in_store_order_id = "MP-MULTI"
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(mercadoPagoResponse))
        };

_mockOrderServiceClient
            .Setup(c => c.GetOrderByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _service.CreatePaymentAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.PaymentId.Should().Be("MP-MULTI");
        _mockOrderServiceClient.Verify(c => c.GetOrderByIdAsync(orderId), Times.Once);
    }

    [Fact]
    public async Task CreatePaymentAsync_WithNullItemDescription_HandlesGracefully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = new OrderResponseDto
        {
            Id = orderId,
            TotalAmount = 100.00m,
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductName = "Item without description",
                    ProductDescription = null,
                    Quantity = 1,
                    UnitPrice = 100.00m,
                    TotalAmount = 100.00m
                }
            }
        };

        var mercadoPagoResponse = new
        {
            qr_data = "pix-data",
            in_store_order_id = "MP-NULL-DESC"
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(mercadoPagoResponse))
        };

_mockOrderServiceClient
            .Setup(c => c.GetOrderByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _service.CreatePaymentAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.PaymentId.Should().Be("MP-NULL-DESC");
    }

    [Fact]
    public async Task CreatePaymentAsync_GeneratesValidQrCodeBase64()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = new OrderResponseDto
        {
            Id = orderId,
            TotalAmount = 50.00m,
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Item",
                    Quantity = 1,
                    UnitPrice = 50.00m,
                    TotalAmount = 50.00m
                }
            }
        };

        var mercadoPagoResponse = new
        {
            qr_data = "00020126580014br.gov.bcb.pix",
            in_store_order_id = "MP-QR"
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(mercadoPagoResponse))
        };

_mockOrderServiceClient
            .Setup(c => c.GetOrderByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _service.CreatePaymentAsync(orderId);

        // Assert
        result.QrCodeBase64.Should().NotBeNullOrEmpty();
        result.QrCodeBase64.Should().MatchRegex("^[A-Za-z0-9+/]*={0,2}$"); // Valid base64
    }

    [Fact]
    public async Task CreatePaymentAsync_UsesCorrectMercadoPagoEndpoint()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = new OrderResponseDto
        {
            Id = orderId,
            TotalAmount = 75.00m,
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test",
                    Quantity = 1,
                    UnitPrice = 75.00m,
                    TotalAmount = 75.00m
                }
            }
        };

        var mercadoPagoResponse = new
        {
            qr_data = "test-qr",
            in_store_order_id = "MP-TEST"
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(mercadoPagoResponse))
        };

        HttpRequestMessage? capturedRequest = null;

_mockOrderServiceClient
            .Setup(c => c.GetOrderByIdAsync(orderId))
            .ReturnsAsync(orderDto);

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(responseMessage);

        // Act
        await _service.CreatePaymentAsync(orderId);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri!.Host.Should().Be("api.mercadopago.com");
        capturedRequest.RequestUri!.AbsolutePath.Should().Contain("/instore/orders/qr/seller/collectors/");
        capturedRequest.Method.Should().Be(HttpMethod.Put);
    }
}
