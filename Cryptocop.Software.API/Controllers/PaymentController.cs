using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Controllers;

[Route("api/payments")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    private string? GetEmail()
    {
        return User.FindFirst("email")?.Value
            ?? User.FindFirst(ClaimTypes.Email)?.Value
            ?? User.FindFirst("emails")?.Value;
    }

    // GET /api/payments
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<PaymentCardDto>>> Get()
    {
        var email = GetEmail();
        if (string.IsNullOrWhiteSpace(email)) return Unauthorized();
    var cards = await _paymentService.GetStoredPaymentCards(email);
        return Ok(cards);
    }

    // POST /api/payments
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Post([FromBody] PaymentCardInputModel input)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var email = GetEmail();
        if (string.IsNullOrWhiteSpace(email)) return Unauthorized();
    await _paymentService.AddPaymentCard(email, input);
        return Created("/api/payments", null);
    }
}