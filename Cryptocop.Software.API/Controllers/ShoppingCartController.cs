using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Services.Interfaces;
using System.Security.Claims;

namespace Cryptocop.Software.API.Controllers;

[Route("api/cart")]
[ApiController]
public class ShoppingCartController : ControllerBase
{
    private readonly IShoppingCartService _cartService;

    public ShoppingCartController(IShoppingCartService cartService)
    {
        _cartService = cartService;
    }

    private string? GetEmail()
    {
        return User.FindFirst("email")?.Value
            ?? User.FindFirst(ClaimTypes.Email)?.Value
            ?? User.FindFirst("emails")?.Value;
    }

    // GET /api/cart
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ShoppingCartItemDto>>> GetCart()
    {
        var email = GetEmail();
        if (string.IsNullOrWhiteSpace(email)) return Unauthorized();
    var items = await _cartService.GetCartItems(email);
        return Ok(items);
    }

    // POST /api/cart
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddItem([FromBody] ShoppingCartItemInputModel input)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var email = GetEmail();
        if (string.IsNullOrWhiteSpace(email)) return Unauthorized();

    await _cartService.AddCartItem(email, input);
        return Created("/api/cart", null);
    }

    // DELETE /api/cart/{id}
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> RemoveItem([FromRoute] int id)
    {
        var email = GetEmail();
        if (string.IsNullOrWhiteSpace(email)) return Unauthorized();
    await _cartService.RemoveCartItem(email, id);
        return NoContent();
    }

    // PATCH /api/cart/{id}
    [HttpPatch("{id:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateQuantity([FromRoute] int id, [FromBody] UpdateQuantityInputModel input)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var email = GetEmail();
        if (string.IsNullOrWhiteSpace(email)) return Unauthorized();
    await _cartService.UpdateCartItemQuantity(email, id, input.Quantity!.Value);
        return NoContent();
    }

    // DELETE /api/cart
    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Clear()
    {
        var email = GetEmail();
        if (string.IsNullOrWhiteSpace(email)) return Unauthorized();
    await _cartService.ClearCart(email);
        return NoContent();
    }
}