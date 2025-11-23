using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using TechTalk.SpecFlow;

namespace ManaFoodPayment.BDD
{
    [Binding]
    public class PaymentSteps
    {
        private readonly ScenarioContext _context;
        private HttpResponseMessage _response;
        private string _orderId;

        public PaymentSteps(ScenarioContext context)
        {
            _context = context;
        }

        [Given("an order exists with id \"(.*)\"")]
        public void GivenAnOrderExistsWithId(string orderId)
        {
            _orderId = orderId;
            // Criar o pedido via API ou mockar aqui, se necessário
        }

        [When("I request a payment for order \"(.*)\"")]
        public async Task WhenIRequestAPaymentForOrder(string orderId)
        {
            using var client = new HttpClient();
            var url = $"http://localhost:5000/api/payment?orderId={orderId}";
            _response = await client.PostAsync(url, null);
        }

        [Then("the payment response should contain a PaymentId, QrData, and QrCodeBase64")]
        public async Task ThenThePaymentResponseShouldContainFields()
        {
            _response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JObject.Parse(await _response.Content.ReadAsStringAsync());
            json["paymentId"].Should().NotBeNullOrEmpty();
            json["qrData"].Should().NotBeNullOrEmpty();
            json["qrCodeBase64"].Should().NotBeNullOrEmpty();
        }
    }
}
