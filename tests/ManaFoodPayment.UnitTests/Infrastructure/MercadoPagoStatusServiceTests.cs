using FluentAssertions;
using ManaFoodPayment.Application.Interfaces;
using ManaFoodPayment.Infrastructure.Services;
using Moq;
using Moq.Protected;
using System.Net;
using Xunit;

namespace ManaFoodPayment.UnitTests.Infrastructure;

public class MercadoPagoStatusServiceTests
{
    private readonly Mock<IPaymentProviderConfig> _mockConfig;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;

    public MercadoPagoStatusServiceTests()
    {
        _mockConfig = new Mock<IPaymentProviderConfig>();
        _mockConfig.Setup(c => c.AccessToken).Returns("test_token");

        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
    }

    [Fact]
    public async Task GetPaymentStatusAsync_WithApprovedPayment_ReturnsStatusAndOrderId()
    {
        var paymentId = "12345";
        var orderId = Guid.NewGuid().ToString();
        var responseContent = $@"{{
            ""status"": ""approved"",
            ""external_reference"": ""{orderId}""
        }}";

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            });

        var service = new MercadoPagoStatusService(_httpClient, _mockConfig.Object);

        var result = await service.GetPaymentStatusAsync(paymentId);

        result.status.Should().Be("approved");
        result.orderId.Should().Be(orderId);
    }

    [Fact]
    public async Task GetPaymentStatusAsync_WithRejectedPayment_ReturnsRejectedStatus()
    {
        var paymentId = "12346";
        var orderId = Guid.NewGuid().ToString();
        var responseContent = $@"{{
            ""status"": ""rejected"",
            ""external_reference"": ""{orderId}""
        }}";

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            });

        var service = new MercadoPagoStatusService(_httpClient, _mockConfig.Object);

        var result = await service.GetPaymentStatusAsync(paymentId);

        result.status.Should().Be("rejected");
        result.orderId.Should().Be(orderId);
    }

    [Fact]
    public async Task GetPaymentStatusAsync_SetsAuthorizationHeader()
    {
        var paymentId = "12347";
        var orderId = Guid.NewGuid().ToString();
        var responseContent = $@"{{
            ""status"": ""approved"",
            ""external_reference"": ""{orderId}""
        }}";

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
                Content = new StringContent(responseContent)
            });

        var service = new MercadoPagoStatusService(_httpClient, _mockConfig.Object);

        await service.GetPaymentStatusAsync(paymentId);

        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.Authorization.Should().NotBeNull();
        capturedRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
        capturedRequest.Headers.Authorization.Parameter.Should().Be("test_token");
    }

    [Fact]
    public async Task GetPaymentStatusAsync_UsesCorrectEndpoint()
    {
        var paymentId = "12348";
        var orderId = Guid.NewGuid().ToString();
        var responseContent = $@"{{
            ""status"": ""approved"",
            ""external_reference"": ""{orderId}""
        }}";

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
                Content = new StringContent(responseContent)
            });

        var service = new MercadoPagoStatusService(_httpClient, _mockConfig.Object);

        await service.GetPaymentStatusAsync(paymentId);

        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri.Should().NotBeNull();
        capturedRequest.RequestUri!.ToString().Should().Be($"https://api.mercadopago.com/v1/payments/{paymentId}");
    }

    [Fact]
    public async Task GetPaymentStatusAsync_WithErrorResponse_ThrowsException()
    {
        var paymentId = "12349";

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

        var service = new MercadoPagoStatusService(_httpClient, _mockConfig.Object);

        await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await service.GetPaymentStatusAsync(paymentId));
    }
}
