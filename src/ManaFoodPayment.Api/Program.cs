using ManaFoodPayment.Application.Interfaces;
using ManaFoodPayment.Infrastructure.Configurations;
using ManaFoodPayment.Infrastructure.Repositories;
using ManaFoodPayment.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<PaymentProviderConfig>(builder.Configuration.GetSection("PaymentProvider"));

// HttpClients and DI
builder.Services.AddHttpClient<IPaymentService, PaymentService>();
builder.Services.AddScoped<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddScoped<IPaymentProviderConfig, PaymentProviderConfig>(sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<PaymentProviderConfig>>().Value);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();