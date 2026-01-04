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
    public async Task<ActionResult<CreatePaymentResponseDto>> Create([FromBody] CreatePaymentRequestDto request)
    {
        var response = await _paymentService.CreatePaymentAsync(request.OrderId);
        return Ok(response);
    }

    [HttpGet("qr-image/{orderId}")]
    public async Task<IActionResult> GetQrImage(Guid orderId)
    {
        var response = await _paymentService.CreatePaymentAsync(orderId);
        var imageBytes = Convert.FromBase64String(response.QrCodeBase64);
        return File(imageBytes, "image/png");
    }
}
