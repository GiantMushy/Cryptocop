using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Controllers;

[Route("api/orders")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    private string? GetEmail()
    {
        return User.FindFirst("email")?.Value
            ?? User.FindFirst(ClaimTypes.Email)?.Value
            ?? User.FindFirst("emails")?.Value;
    }

    // GET /api/orders - all orders for authenticated user
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<OrderDto>>> Get()
    {
        var email = GetEmail();
        if (string.IsNullOrWhiteSpace(email)) return Unauthorized();
    var orders = await _orderService.GetOrders(email);
        return Ok(orders);
    }

    // POST /api/orders - create new order for authenticated user
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<OrderDto>> Post([FromBody] OrderInputModel input)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var email = GetEmail();
        if (string.IsNullOrWhiteSpace(email)) return Unauthorized();
    var created = await _orderService.CreateNewOrder(email, input);
        return Created($"/api/orders/{created.Id}", created);
    }
}