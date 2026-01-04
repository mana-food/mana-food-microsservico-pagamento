using FluentAssertions;
using ManaFoodPayment.Application.Dtos;
using ManaFoodPayment.Application.Interfaces;
using ManaFoodPayment.Infrastructure.Configurations;
using ManaFoodPayment.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;
using TechTalk.SpecFlow;

namespace ManaFoodPayment.BDD.Steps;

[Binding]
public class PaymentSteps
{
    private readonly Mock<IOrderServiceClient> _mockOrderServiceClient;
    private readonly Mock<IPaymentStatusService> _mockPaymentStatusService;
    private readonly Mock<IPaymentProviderConfig> _mockConfig;
    private readonly Mock<ILogger<PaymentService>> _mockPaymentLogger;
    private readonly Mock<ILogger<WebhookService>> _mockWebhookLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private HttpClient _httpClient = null!;
    
    private IPaymentService _paymentService = null!;
    private IWebhookService _webhookService = null!;
    
    private CreatePaymentResponseDto? _paymentResponse;
    private Exception? _exception;
    private Guid _orderId;
    private string _paymentId = string.Empty;
    private string _paymentStatus = string.Empty;
    private string _externalReference = string.Empty;

    public PaymentSteps()
    {
        _mockOrderServiceClient = new Mock<IOrderServiceClient>();
        _mockPaymentStatusService = new Mock<IPaymentStatusService>();
        _mockConfig = new Mock<IPaymentProviderConfig>();
        _mockPaymentLogger = new Mock<ILogger<PaymentService>>();
        _mockWebhookLogger = new Mock<ILogger<WebhookService>>();
        _mockHttpHandler = new Mock<HttpMessageHandler>();
    }

    #region Given Steps

    [Given(@"que existe um pedido com ID ""(.*)"" no Order Service")]
    public void DadoQueExisteUmPedidoComID(string orderIdStr)
    {
        _orderId = Guid.Parse(orderIdStr);
        
        var orderDto = new OrderResponseDto
        {
            Id = _orderId,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Cliente Teste BDD",
            OrderDate = DateTime.UtcNow.AddMinutes(-10),
            Status = "Pending",
            TotalAmount = 85.50m,
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "X-Burger",
                    ProductDescription = "Hambúrguer artesanal",
                    Quantity = 2,
                    UnitPrice = 28.90m,
                    TotalAmount = 57.80m
                },
                new OrderItemDto
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Batata Frita",
                    ProductDescription = "Porção grande",
                    Quantity = 1,
                    UnitPrice = 27.70m,
                    TotalAmount = 27.70m
                }
            }
        };

        _mockOrderServiceClient
            .Setup(x => x.GetOrderByIdAsync(_orderId))
            .ReturnsAsync(orderDto);
    }

    [Given(@"o pedido contém (.*) itens no valor total de R\$ (.*)")]
    public void DadoOPedidoContemItensNoValorTotal(int itemCount, decimal totalAmount)
    {
        // Validação já configurada no step anterior
    }

    [Given(@"que o MercadoPago enviou uma confirmação de pagamento com ID ""(.*)""")]
    public void DadoQueOMercadoPagoEnviouUmaConfirmacaoDePagamentoComID(string paymentId)
    {
        _paymentId = paymentId;
    }

    [Given(@"o pagamento está com status ""(.*)"" na API do MercadoPago")]
    public void DadoOPagamentoEstaComStatusNaAPIDoMercadoPago(string status)
    {
        _paymentStatus = status;
        
        _mockPaymentStatusService
            .Setup(x => x.GetPaymentStatusAsync(_paymentId))
            .ReturnsAsync((status, _externalReference));
    }

    [Given(@"o external_reference é ""(.*)""")]
    public void DadoOExternalReferenceE(string externalRef)
    {
        _externalReference = externalRef;
        
        // Reconfigure o mock com o external_reference correto
        _mockPaymentStatusService
            .Setup(x => x.GetPaymentStatusAsync(_paymentId))
            .ReturnsAsync((_paymentStatus, _externalReference));
    }

    [Given(@"que não existe um pedido com ID ""(.*)"" no Order Service")]
    public void DadoQueNaoExisteUmPedidoComIDNoOrderService(string orderIdStr)
    {
        _orderId = Guid.Parse(orderIdStr);
        
        _mockOrderServiceClient
            .Setup(x => x.GetOrderByIdAsync(_orderId))
            .ReturnsAsync((OrderResponseDto?)null);
    }

    #endregion

    #region When Steps

    [When(@"eu solicito a geração do QR Code para pagamento")]
    public async Task QuandoEuSolicitoAGeracaoDoQRCodeParaPagamento()
    {
        // Configurar mock do HttpClient para MercadoPago API
        var mercadoPagoResponse = new
        {
            qr_data = "00020126580014br.gov.bcb.pix0136123e4567-e12b-12d1-a456-426655440000",
            in_store_order_id = "MP-12345678"
        };

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(mercadoPagoResponse), Encoding.UTF8, "application/json")
        };

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        _httpClient = new HttpClient(_mockHttpHandler.Object);

        _mockConfig.Setup(x => x.AccessToken).Returns("TEST_ACCESS_TOKEN");
        _mockConfig.Setup(x => x.UserId).Returns("TEST_USER_ID");
        _mockConfig.Setup(x => x.ExternalPosId).Returns("TEST_POS_ID");
        _mockConfig.Setup(x => x.NotificationUrl).Returns("https://webhook.test.com");

        _paymentService = new PaymentService(
            _httpClient,
            _mockConfig.Object,
            _mockOrderServiceClient.Object,
            _mockPaymentLogger.Object);

        try
        {
            _paymentResponse = await _paymentService.CreatePaymentAsync(_orderId);
        }
        catch (Exception ex)
        {
            _exception = ex;
        }
    }

    [When(@"o webhook processa a confirmação")]
    public async Task QuandoOWebhookProcessaAConfirmacao()
    {
        _webhookService = new WebhookService(
            _mockPaymentStatusService.Object,
            _mockWebhookLogger.Object);

        try
        {
            await _webhookService.ProcessPaymentConfirmationAsync(_paymentId);
        }
        catch (Exception ex)
        {
            _exception = ex;
        }
    }

    [When(@"eu tento gerar o QR Code para pagamento")]
    public async Task QuandoEuTentoGerarOQRCodeParaPagamento()
    {
        _httpClient = new HttpClient(_mockHttpHandler.Object);

        _mockConfig.Setup(x => x.AccessToken).Returns("TEST_ACCESS_TOKEN");
        _mockConfig.Setup(x => x.UserId).Returns("TEST_USER_ID");
        _mockConfig.Setup(x => x.ExternalPosId).Returns("TEST_POS_ID");
        _mockConfig.Setup(x => x.NotificationUrl).Returns("https://webhook.test.com");

        _paymentService = new PaymentService(
            _httpClient,
            _mockConfig.Object,
            _mockOrderServiceClient.Object,
            _mockPaymentLogger.Object);

        try
        {
            _paymentResponse = await _paymentService.CreatePaymentAsync(_orderId);
        }
        catch (Exception ex)
        {
            _exception = ex;
        }
    }

    #endregion

    #region Then Steps

    [Then(@"o sistema deve retornar um QR Code válido")]
    public void EntaoOSistemaDeveRetornarUmQRCodeValido()
    {
        _paymentResponse.Should().NotBeNull();
    }

    [Then(@"o paymentId deve ser retornado")]
    public void EntaoOPaymentIdDeveSerRetornado()
    {
        _paymentResponse!.PaymentId.Should().NotBeNullOrEmpty();
    }

    [Then(@"o QrData deve ser retornado")]
    public void EntaoOQrDataDeveSerRetornado()
    {
        _paymentResponse!.QrData.Should().NotBeNullOrEmpty();
    }

    [Then(@"o QrCodeBase64 deve conter dados válidos")]
    public void EntaoOQrCodeBase64DeveConterDadosValidos()
    {
        _paymentResponse!.QrCodeBase64.Should().NotBeNullOrEmpty();
        // Validar que é um Base64 válido
        try
        {
            Convert.FromBase64String(_paymentResponse.QrCodeBase64);
        }
        catch
        {
            Assert.Fail("QrCodeBase64 não é um Base64 válido");
        }
    }

    [Then(@"o sistema deve registrar a aprovação do pagamento")]
    public void EntaoOSistemaDeveRegistrarAAprovacaoDoPagamento()
    {
        _mockPaymentStatusService.Verify(
            x => x.GetPaymentStatusAsync(_paymentId),
            Times.Once);
    }

    [Then(@"deve extrair o OrderId do external_reference")]
    public void EntaoDeveExtrairOOrderIdDoExternalReference()
    {
        // Verificar que o external_reference foi obtido
        _mockPaymentStatusService.Verify(
            x => x.GetPaymentStatusAsync(_paymentId),
            Times.Once);
    }

    [Then(@"o sistema não deve processar o pagamento")]
    public void EntaoOSistemaNaoDeveProcessarOPagamento()
    {
        // Verificação implícita: método chamado e não ocorreram erros
        _exception.Should().BeNull();
    }

    [Then(@"deve registrar o status como não aprovado")]
    public void EntaoDeveRegistrarOStatusComoNaoAprovado()
    {
        _mockPaymentStatusService.Verify(
            x => x.GetPaymentStatusAsync(_paymentId),
            Times.Once);
    }

    [Then(@"o sistema deve retornar um erro informando que o pedido não foi encontrado")]
    public void EntaoOSistemaDeveRetornarUmErroInformandoQueOPedidoNaoFoiEncontrado()
    {
        _exception.Should().NotBeNull();
        _exception!.Message.Should().Contain("não encontrado");
    }

    #endregion
}
