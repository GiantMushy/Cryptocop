using Microsoft.AspNetCore.Mvc;
using Cryptocop.Software.API.Services.Interfaces;
using Cryptocop.Software.API.Models.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace Cryptocop.Software.API.Controllers;

[ApiController]
[Route("api/cryptocurrencies")]
[AllowAnonymous]
public class CryptoCurrencyController : ControllerBase
{
    private readonly ICryptoCurrencyService _cryptoService;

    public CryptoCurrencyController(ICryptoCurrencyService cryptoService)
    {
        _cryptoService = cryptoService;
    }

    // GET /api/cryptocurrencies
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CryptoCurrencyDto>>> Get()
    {
    var items = await _cryptoService.GetAvailableCryptocurrencies();
        return Ok(items);
    }

    // GET /api/cryptocurrencies/{identifier}/price
    [HttpGet("{identifier}/price")]
    public async Task<ActionResult<object>> GetPrice([FromRoute] string identifier)
    {
    var price = await _cryptoService.GetPriceUsd(identifier);
        if (price is null) return NotFound();
        return Ok(new { priceUsd = price, priceInUsd = price });
    }
}