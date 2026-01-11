using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ManaFoodPayment.Api.Controllers;
using ManaFoodPayment.Api.Webhooks.MercadoPago;
using ManaFoodPayment.Application.Interfaces;
using Moq;

namespace ManaFoodPayment.UnitTests.Api;

public class TestControllerTests
{
    private readonly Mock<IWebhookService> _mockWebhookService;
    private readonly Mock<ILogger<TestController>> _mockLogger;
    private readonly TestController _controller;

    public TestControllerTests()
    {
        _mockWebhookService = new Mock<IWebhookService>();
        _mockLogger = new Mock<ILogger<TestController>>();
        _controller = new TestController(_mockWebhookService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task SimulateWebhook_WithValidPayload_ReturnsOkWithSuccessMessage()
    {
        // Arrange
        var payload = new MercadoPagoWebhookPayload
        {
            Data = new MercadoPagoData { Id = "MP123456789" }
        };

        _mockWebhookService
            .Setup(s => s.ProcessPaymentConfirmationAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.SimulateWebhook(payload);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
        
        _mockWebhookService.Verify(
            s => s.ProcessPaymentConfirmationAsync("MP123456789"), 
            Times.Once
        );
    }

    [Fact]
    public async Task SimulateWebhook_WithNullPayload_ReturnsBadRequest()
    {
        // Arrange
        MercadoPagoWebhookPayload? payload = null;

        // Act
        var result = await _controller.SimulateWebhook(payload!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _mockWebhookService.Verify(
            s => s.ProcessPaymentConfirmationAsync(It.IsAny<string>()), 
            Times.Never
        );
    }

    [Fact]
    public async Task SimulateWebhook_WithNullData_ReturnsBadRequest()
    {
        // Arrange
        var payload = new MercadoPagoWebhookPayload { Data = null };

        // Act
        var result = await _controller.SimulateWebhook(payload);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task SimulateWebhook_WithEmptyPaymentId_ReturnsBadRequest()
    {
        // Arrange
        var payload = new MercadoPagoWebhookPayload
        {
            Data = new MercadoPagoData { Id = "" }
        };

        // Act
        var result = await _controller.SimulateWebhook(payload);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _mockWebhookService.Verify(
            s => s.ProcessPaymentConfirmationAsync(It.IsAny<string>()), 
            Times.Never
        );
    }

    [Fact]
    public async Task SimulateWebhook_WhenServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        var payload = new MercadoPagoWebhookPayload
        {
            Data = new MercadoPagoData { Id = "MP123456789" }
        };

        _mockWebhookService
            .Setup(s => s.ProcessPaymentConfirmationAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Payment processing failed"));

        // Act
        var result = await _controller.SimulateWebhook(payload);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public void GetWebhookPayloadExample_ReturnsOkWithExample()
    {
        // Act
        var result = _controller.GetWebhookPayloadExample();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public void Health_ReturnsOkWithHealthStatus()
    {
        // Act
        var result = _controller.Health();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
    }
}
