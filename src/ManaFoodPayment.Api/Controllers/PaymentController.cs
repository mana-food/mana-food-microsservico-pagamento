using Microsoft.AspNetCore.Mvc;
using ManaFoodPayment.Application.Dtos;
using ManaFoodPayment.Application.Interfaces;

namespace ManaFoodPayment.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("create")]
    public async Task<ActionResult<CreatePaymentResponse>> Create([FromBody] CreatePaymentRequest request)
    {
        var response = await _paymentService.CreatePaymentAsync(request.OrderId);
        return Ok(response);
    }
}
