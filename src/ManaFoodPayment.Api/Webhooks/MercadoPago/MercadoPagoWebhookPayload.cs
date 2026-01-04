namespace ManaFoodPayment.Api.Webhooks.MercadoPago;

public class MercadoPagoWebhookPayload
{
    public MercadoPagoData? Data { get; set; }
}

public class MercadoPagoData
{
    public string Id { get; set; } = string.Empty;
}
