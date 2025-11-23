using System.Threading.Tasks;
using FluentAssertions;
using ManaFoodPayment.Infrastructure.Services;
using ManaFoodPayment.Application.Dtos;
using NUnit.Framework;

namespace ManaFoodPayment.UnitTest
{
    [TestFixture]
    public class PaymentServiceTests
    {
        [Test]
        public async Task CreatePaymentAsync_ShouldReturnValidResponse()
        {
            // Arrange
            var service = new PaymentService();
            var orderId = "order-123";

            // Act
            var response = await service.CreatePaymentAsync(orderId);

            // Assert
            response.Should().NotBeNull();
            response.PaymentId.Should().NotBeNullOrEmpty();
            response.QrData.Should().NotBeNullOrEmpty();
            response.QrCodeBase64.Should().NotBeNullOrEmpty();
        }
    }
}
