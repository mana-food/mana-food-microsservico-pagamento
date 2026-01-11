using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ManaFoodPayment.Api.Webhooks.MercadoPago;
using ManaFoodPayment.Application.Interfaces;
using Moq;

namespace ManaFoodPayment.UnitTests.Api.Webhooks;

public class MercadoPagoWebhookReceiverTests
{
    private readonly Mock<IWebhookService> _mockWebhookService;
    private readonly Mock<ILogger<MercadoPagoWebhookReceiver>> _mockLogger;
    private readonly MercadoPagoWebhookReceiver _controller;

    public MercadoPagoWebhookReceiverTests()
    {
        _mockWebhookService = new Mock<IWebhookService>();
        _mockLogger = new Mock<ILogger<MercadoPagoWebhookReceiver>>();
        _controller = new MercadoPagoWebhookReceiver(_mockWebhookService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ReceivePaymentConfirmation_WithValidPayload_ReturnsOk()
    {
        // Arrange
        var payload = new MercadoPagoWebhookPayload
        {
            Data = new MercadoPagoData { Id = "MP987654321" }
        };

        _mockWebhookService
            .Setup(s => s.ProcessPaymentConfirmationAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ReceivePaymentConfirmation(payload);

        // Assert
        result.Should().BeOfType<OkResult>();
        _mockWebhookService.Verify(
            s => s.ProcessPaymentConfirmationAsync("MP987654321"), 
            Times.Once
        );
    }

    [Fact]
    public async Task ReceivePaymentConfirmation_WithNullPayload_ReturnsBadRequest()
    {
        // Arrange
        MercadoPagoWebhookPayload? payload = null;

        // Act
        var result = await _controller.ReceivePaymentConfirmation(payload!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Invalid payload");
        
        _mockWebhookService.Verify(
            s => s.ProcessPaymentConfirmationAsync(It.IsAny<string>()), 
            Times.Never
        );
    }

    [Fact]
    public async Task ReceivePaymentConfirmation_WithNullData_ReturnsBadRequest()
    {
        // Arrange
        var payload = new MercadoPagoWebhookPayload { Data = null };

        // Act
        var result = await _controller.ReceivePaymentConfirmation(payload);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Invalid payload");
    }

    [Fact]
    public async Task ReceivePaymentConfirmation_WithEmptyPaymentId_ReturnsBadRequest()
    {
        // Arrange
        var payload = new MercadoPagoWebhookPayload
        {
            Data = new MercadoPagoData { Id = "" }
        };

        // Act
        var result = await _controller.ReceivePaymentConfirmation(payload);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _mockWebhookService.Verify(
            s => s.ProcessPaymentConfirmationAsync(It.IsAny<string>()), 
            Times.Never
        );
    }

    [Fact]
    public async Task ReceivePaymentConfirmation_WithWhitespacePaymentId_ReturnsBadRequest()
    {
        // Arrange
        var payload = new MercadoPagoWebhookPayload
        {
            Data = new MercadoPagoData { Id = "   " }
        };

        // Act
        var result = await _controller.ReceivePaymentConfirmation(payload);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _mockWebhookService.Verify(
            s => s.ProcessPaymentConfirmationAsync(It.IsAny<string>()), 
            Times.Never
        );
    }

    [Fact]
    public async Task ReceivePaymentConfirmation_CallsWebhookService_WithCorrectPaymentId()
    {
        // Arrange
        var expectedPaymentId = "MP111222333";
        var payload = new MercadoPagoWebhookPayload
        {
            Data = new MercadoPagoData { Id = expectedPaymentId }
        };

        _mockWebhookService
            .Setup(s => s.ProcessPaymentConfirmationAsync(expectedPaymentId))
            .Returns(Task.CompletedTask);

        // Act
        await _controller.ReceivePaymentConfirmation(payload);

        // Assert
        _mockWebhookService.Verify(
            s => s.ProcessPaymentConfirmationAsync(expectedPaymentId), 
            Times.Once
        );
    }
}
