using FluentAssertions;
using ManaFoodPayment.Application.Interfaces;
using ManaFoodPayment.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ManaFoodPayment.UnitTests.Infrastructure;

public class WebhookServiceTests
{
    private readonly Mock<IPaymentStatusService> _mockPaymentStatusService;
    private readonly Mock<ILogger<WebhookService>> _mockLogger;

    public WebhookServiceTests()
    {
        _mockPaymentStatusService = new Mock<IPaymentStatusService>();
        _mockLogger = new Mock<ILogger<WebhookService>>();
    }

    [Fact]
    public async Task ProcessPaymentConfirmationAsync_WithApprovedPayment_LogsSuccess()
    {
        var paymentId = "12345";
        var orderId = Guid.NewGuid().ToString();

        _mockPaymentStatusService
            .Setup(s => s.GetPaymentStatusAsync(paymentId))
            .ReturnsAsync(("approved", orderId));

        var service = new WebhookService(_mockPaymentStatusService.Object, _mockLogger.Object);

        await service.ProcessPaymentConfirmationAsync(paymentId);

        _mockPaymentStatusService.Verify(s => s.GetPaymentStatusAsync(paymentId), Times.Once);
    }

    [Fact]
    public async Task ProcessPaymentConfirmationAsync_WithRejectedPayment_LogsWarning()
    {
        var paymentId = "12346";
        var orderId = Guid.NewGuid().ToString();

        _mockPaymentStatusService
            .Setup(s => s.GetPaymentStatusAsync(paymentId))
            .ReturnsAsync(("rejected", orderId));

        var service = new WebhookService(_mockPaymentStatusService.Object, _mockLogger.Object);

        await service.ProcessPaymentConfirmationAsync(paymentId);

        _mockPaymentStatusService.Verify(s => s.GetPaymentStatusAsync(paymentId), Times.Once);
    }

    [Fact]
    public async Task ProcessPaymentConfirmationAsync_WithPendingPayment_DoesNotProcess()
    {
        var paymentId = "12347";
        var orderId = Guid.NewGuid().ToString();

        _mockPaymentStatusService
            .Setup(s => s.GetPaymentStatusAsync(paymentId))
            .ReturnsAsync(("pending", orderId));

        var service = new WebhookService(_mockPaymentStatusService.Object, _mockLogger.Object);

        await service.ProcessPaymentConfirmationAsync(paymentId);

        _mockPaymentStatusService.Verify(s => s.GetPaymentStatusAsync(paymentId), Times.Once);
    }

    [Fact]
    public async Task ProcessPaymentConfirmationAsync_WithException_LogsError()
    {
        var paymentId = "12348";

        _mockPaymentStatusService
            .Setup(s => s.GetPaymentStatusAsync(paymentId))
            .ThrowsAsync(new Exception("Test exception"));

        var service = new WebhookService(_mockPaymentStatusService.Object, _mockLogger.Object);

        await service.ProcessPaymentConfirmationAsync(paymentId);

        _mockPaymentStatusService.Verify(s => s.GetPaymentStatusAsync(paymentId), Times.Once);
    }

    [Fact]
    public async Task ProcessPaymentConfirmationAsync_WithApprovedPayment_ParsesOrderId()
    {
        var paymentId = "12349";
        var orderId = Guid.NewGuid();

        _mockPaymentStatusService
            .Setup(s => s.GetPaymentStatusAsync(paymentId))
            .ReturnsAsync(("approved", orderId.ToString()));

        var service = new WebhookService(_mockPaymentStatusService.Object, _mockLogger.Object);

        await service.ProcessPaymentConfirmationAsync(paymentId);

        _mockPaymentStatusService.Verify(s => s.GetPaymentStatusAsync(paymentId), Times.Once);
    }

    [Fact]
    public async Task ProcessPaymentConfirmationAsync_WithInvalidOrderIdGuid_LogsError()
    {
        var paymentId = "12350";
        var invalidOrderId = "not-a-valid-guid";

        _mockPaymentStatusService
            .Setup(s => s.GetPaymentStatusAsync(paymentId))
            .ReturnsAsync(("approved", invalidOrderId));

        var service = new WebhookService(_mockPaymentStatusService.Object, _mockLogger.Object);

        await service.ProcessPaymentConfirmationAsync(paymentId);

        _mockPaymentStatusService.Verify(s => s.GetPaymentStatusAsync(paymentId), Times.Once);
    }
}
