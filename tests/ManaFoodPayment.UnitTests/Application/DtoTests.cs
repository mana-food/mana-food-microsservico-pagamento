using FluentAssertions;
using ManaFoodPayment.Application.Dtos;
using ManaFoodPayment.Domain.Enums;

namespace ManaFoodPayment.UnitTests.Application;

public class DtoTests
{
    [Fact]
    public void CreatePaymentRequestDto_ShouldSetOrderIdCorrectly()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Act
        var dto = new CreatePaymentRequestDto { OrderId = orderId };

        // Assert
        dto.OrderId.Should().Be(orderId);
    }

    [Fact]
    public void CreatePaymentResponseDto_ShouldSetAllPropertiesCorrectly()
    {
        // Arrange & Act
        var dto = new CreatePaymentResponseDto
        {
            PaymentId = "MP123456",
            QrData = "00020126580014br.gov.bcb.pix",
            QrCodeBase64 = "iVBORw0KGgoAAAANSU="
        };

        // Assert
        dto.PaymentId.Should().Be("MP123456");
        dto.QrData.Should().Be("00020126580014br.gov.bcb.pix");
        dto.QrCodeBase64.Should().Be("iVBORw0KGgoAAAANSU=");
    }

    [Fact]
    public void PaymentDto_ShouldSetAllPropertiesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var confirmedAt = DateTime.UtcNow.AddMinutes(5);

        // Act
        var dto = new PaymentDto
        {
            Id = id,
            OrderId = orderId,
            PaymentId = "MP789",
            QrData = "qr-data",
            Amount = 150.00m,
            CreatedAt = createdAt,
            ConfirmedAt = confirmedAt
        };

        // Assert
        dto.Id.Should().Be(id);
        dto.OrderId.Should().Be(orderId);
        dto.PaymentId.Should().Be("MP789");
        dto.QrData.Should().Be("qr-data");
        dto.Amount.Should().Be(150.00m);
        dto.CreatedAt.Should().Be(createdAt);
        dto.ConfirmedAt.Should().Be(confirmedAt);
    }

    [Fact]
    public void CreatePaymentDto_ShouldSetAllPropertiesCorrectly()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Act
        var dto = new CreatePaymentDto
        {
            OrderId = orderId,
            PaymentId = "MP999",
            QrData = "pix-data",
            Amount = 200.00m
        };

        // Assert
        dto.OrderId.Should().Be(orderId);
        dto.PaymentId.Should().Be("MP999");
        dto.QrData.Should().Be("pix-data");
        dto.Amount.Should().Be(200.00m);
    }

    [Fact]
    public void OrderResponseDto_ShouldInitializeEmptyItemsList()
    {
        // Act
        var dto = new OrderResponseDto();

        // Assert
        dto.Items.Should().NotBeNull();
        dto.Items.Should().BeEmpty();
    }

    [Fact]
    public void OrderResponseDto_ShouldSetAllPropertiesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var items = new List<OrderItemDto>
        {
            new OrderItemDto
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductName = "Pizza",
                Quantity = 2,
                UnitPrice = 50.00m,
                TotalAmount = 100.00m
            }
        };

        // Act
        var dto = new OrderResponseDto
        {
            Id = id,
            TotalAmount = 100.00m,
            CustomerName = "John Doe",
            CustomerEmail = "john@test.com",
            Items = items
        };

        // Assert
        dto.Id.Should().Be(id);
        dto.TotalAmount.Should().Be(100.00m);
        dto.CustomerName.Should().Be("John Doe");
        dto.CustomerEmail.Should().Be("john@test.com");
        dto.Items.Should().HaveCount(1);
        dto.Items.First().ProductName.Should().Be("Pizza");
    }

    [Fact]
    public void OrderItemDto_ShouldSetAllPropertiesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var productId = Guid.NewGuid();

        // Act
        var dto = new OrderItemDto
        {
            Id = id,
            ProductId = productId,
            ProductName = "X-Burger",
            ProductDescription = "Delicious burger",
            Quantity = 3,
            UnitPrice = 25.00m,
            TotalAmount = 75.00m
        };

        // Assert
        dto.Id.Should().Be(id);
        dto.ProductId.Should().Be(productId);
        dto.ProductName.Should().Be("X-Burger");
        dto.ProductDescription.Should().Be("Delicious burger");
        dto.Quantity.Should().Be(3);
        dto.UnitPrice.Should().Be(25.00m);
        dto.TotalAmount.Should().Be(75.00m);
    }

    [Fact]
    public void OrderItemDto_ShouldAllowNullDescription()
    {
        // Act
        var dto = new OrderItemDto
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Item without description",
            ProductDescription = null,
            Quantity = 1,
            UnitPrice = 10.00m,
            TotalAmount = 10.00m
        };

        // Assert
        dto.ProductDescription.Should().BeNull();
        dto.ProductName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void PaymentDto_ConfirmedAt_CanBeNull()
    {
        // Act
        var dto = new PaymentDto
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            PaymentId = "MP111",
            QrData = "data",
            Amount = 50.00m,
            CreatedAt = DateTime.UtcNow,
            ConfirmedAt = null
        };

        // Assert
        dto.ConfirmedAt.Should().BeNull();
    }
}
