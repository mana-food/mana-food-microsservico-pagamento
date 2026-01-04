using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using ManaFoodPayment.Api.Controllers;
using ManaFoodPayment.Application.Dtos;
using ManaFoodPayment.Application.Interfaces;
using Moq;

namespace ManaFoodPayment.UnitTests.Api;

public class PaymentControllerTests
{
    private readonly Mock<IPaymentService> _mockPaymentService;
    private readonly PaymentController _controller;

    public PaymentControllerTests()
    {
        _mockPaymentService = new Mock<IPaymentService>();
        _controller = new PaymentController(_mockPaymentService.Object);
    }

    [Fact]
    public async Task Create_WithValidOrderId_ReturnsOkWithPaymentResponse()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new CreatePaymentRequestDto { OrderId = orderId };
        var expectedResponse = new CreatePaymentResponseDto
        {
            PaymentId = "MP123456",
            QrData = "00020126580014br.gov.bcb.pix...",
            QrCodeBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg=="
        };

        _mockPaymentService
            .Setup(s => s.CreatePaymentAsync(orderId))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Create(request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        _mockPaymentService.Verify(s => s.CreatePaymentAsync(orderId), Times.Once);
    }

    [Fact]
    public async Task GetQrImage_WithValidOrderId_ReturnsImageFile()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var base64Image = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==";
        var expectedResponse = new CreatePaymentResponseDto
        {
            PaymentId = "MP123456",
            QrData = "00020126580014br.gov.bcb.pix...",
            QrCodeBase64 = base64Image
        };

        _mockPaymentService
            .Setup(s => s.CreatePaymentAsync(orderId))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetQrImage(orderId);

        // Assert
        result.Should().BeOfType<FileContentResult>();
        var fileResult = result as FileContentResult;
        fileResult!.ContentType.Should().Be("image/png");
        fileResult.FileContents.Should().BeEquivalentTo(Convert.FromBase64String(base64Image));
        _mockPaymentService.Verify(s => s.CreatePaymentAsync(orderId), Times.Once);
    }

    [Fact]
    public async Task Create_CallsPaymentServiceWithCorrectOrderId()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new CreatePaymentRequestDto { OrderId = orderId };
        var response = new CreatePaymentResponseDto
        {
            PaymentId = "MP999",
            QrData = "pix-data",
            QrCodeBase64 = "base64-data"
        };

        _mockPaymentService
            .Setup(s => s.CreatePaymentAsync(It.IsAny<Guid>()))
            .ReturnsAsync(response);

        // Act
        await _controller.Create(request);

        // Assert
        _mockPaymentService.Verify(s => s.CreatePaymentAsync(orderId), Times.Once);
    }
}
