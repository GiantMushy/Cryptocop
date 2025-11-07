using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Cryptocop.Software.API.Services.Interfaces;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Models.Dtos;

namespace Cryptocop.Software.API.Controllers;

[Route("api/addresses")]
[ApiController]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    private string? GetEmail()
    {
        return User.FindFirst("email")?.Value
            ?? User.FindFirst(ClaimTypes.Email)?.Value
            ?? User.FindFirst("emails")?.Value;
    }

    // GET /api/addresses
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<AddressDto>>> GetAll()
    {
        var email = GetEmail();
        if (string.IsNullOrWhiteSpace(email)) return Unauthorized();
    var items = await _addressService.GetAllAddresses(email);
        return Ok(items);
    }

    // POST /api/addresses
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Add([FromBody] AddressInputModel input)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var email = GetEmail();
        if (string.IsNullOrWhiteSpace(email)) return Unauthorized();
    await _addressService.AddAddress(email, input);
        return Created("/api/addresses", null);
    }

    // DELETE /api/addresses/{id}
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var email = GetEmail();
        if (string.IsNullOrWhiteSpace(email)) return Unauthorized();
    await _addressService.DeleteAddress(email, id);
        return NoContent();
    }
}