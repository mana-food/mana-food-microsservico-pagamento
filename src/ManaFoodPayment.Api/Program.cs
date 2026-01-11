using ManaFoodPayment.Application.Interfaces;
using ManaFoodPayment.Infrastructure.Configurations;
using ManaFoodPayment.Infrastructure.Services;
using System.Reflection;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<PaymentProviderConfig>(builder.Configuration.GetSection("PaymentProvider"));

// Services
builder.Services.AddHttpClient<IPaymentService, PaymentService>();
builder.Services.AddHttpClient<IOrderServiceClient, OrderServiceClient>();
builder.Services.AddHttpClient<IPaymentStatusService, MercadoPagoStatusService>();
builder.Services.AddScoped<IWebhookService, WebhookService>();
builder.Services.AddScoped<IPaymentProviderConfig, PaymentProviderConfig>(sp => 
    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<PaymentProviderConfig>>().Value);


builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ManaFood - Payment Service API",
        Version = "v1",
        Description = "API para gerenciamento de pagamentos via QR Code do Mercado Pago",
        Contact = new OpenApiContact
        {
            Name = "ManaFood Team",
            Url = new Uri("https://github.com/mana-food/mana-food-microsservico-pagamento")
        }
    });

    // Habilitar comentários XML se o arquivo existir
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.MapHealthChecks("/health");

await app.RunAsync();