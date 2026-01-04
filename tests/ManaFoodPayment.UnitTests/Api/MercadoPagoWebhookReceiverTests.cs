using FluentAssertions;
using ManaFoodPayment.Api.Webhooks.MercadoPago;
using ManaFoodPayment.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ManaFoodPayment.UnitTests.Api;

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
        var payload = new MercadoPagoWebhookPayload
        {
            Data = new MercadoPagoData { Id = "12345" }
        };

        var result = await _controller.ReceivePaymentConfirmation(payload);

        result.Should().BeOfType<OkResult>();
        _mockWebhookService.Verify(s => s.ProcessPaymentConfirmationAsync("12345"), Times.Once);
    }

    [Fact]
    public async Task ReceivePaymentConfirmation_WithNullPayload_ReturnsBadRequest()
    {
        var result = await _controller.ReceivePaymentConfirmation(null);

        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Invalid payload");
        _mockWebhookService.Verify(s => s.ProcessPaymentConfirmationAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ReceivePaymentConfirmation_WithNullData_ReturnsBadRequest()
    {
        var payload = new MercadoPagoWebhookPayload { Data = null };

        var result = await _controller.ReceivePaymentConfirmation(payload);

        result.Should().BeOfType<BadRequestObjectResult>();
        _mockWebhookService.Verify(s => s.ProcessPaymentConfirmationAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ReceivePaymentConfirmation_WithEmptyPaymentId_ReturnsBadRequest()
    {
        var payload = new MercadoPagoWebhookPayload
        {
            Data = new MercadoPagoData { Id = "" }
        };

        var result = await _controller.ReceivePaymentConfirmation(payload);

        result.Should().BeOfType<BadRequestObjectResult>();
        _mockWebhookService.Verify(s => s.ProcessPaymentConfirmationAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ReceivePaymentConfirmation_WithWhitespacePaymentId_ReturnsBadRequest()
    {
        var payload = new MercadoPagoWebhookPayload
        {
            Data = new MercadoPagoData { Id = "   " }
        };

        var result = await _controller.ReceivePaymentConfirmation(payload);

        result.Should().BeOfType<BadRequestObjectResult>();
        _mockWebhookService.Verify(s => s.ProcessPaymentConfirmationAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ReceivePaymentConfirmation_CallsWebhookServiceWithCorrectPaymentId()
    {
        var expectedPaymentId = "payment-123456";
        var payload = new MercadoPagoWebhookPayload
        {
            Data = new MercadoPagoData { Id = expectedPaymentId }
        };

        await _controller.ReceivePaymentConfirmation(payload);

        _mockWebhookService.Verify(
            s => s.ProcessPaymentConfirmationAsync(expectedPaymentId),
            Times.Once);
    }

    [Fact]
    public async Task ReceivePaymentConfirmation_WithException_ThrowsException()
    {
        var payload = new MercadoPagoWebhookPayload
        {
            Data = new MercadoPagoData { Id = "12345" }
        };

        _mockWebhookService
            .Setup(s => s.ProcessPaymentConfirmationAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Test exception"));

        await Assert.ThrowsAsync<Exception>(async () =>
            await _controller.ReceivePaymentConfirmation(payload));
    }
}
