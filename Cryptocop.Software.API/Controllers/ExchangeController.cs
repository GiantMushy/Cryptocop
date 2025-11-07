using Microsoft.AspNetCore.Mvc;
using Cryptocop.Software.API.Models;
using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Controllers;

[Route("api/exchanges")]
[ApiController]
public class ExchangeController : ControllerBase
{
    private readonly IExchangeService _exchangeService;

    public ExchangeController(IExchangeService exchangeService)
    {
        _exchangeService = exchangeService;
    }

    // GET /api/exchanges?pageNumber=1
    [HttpGet]
    public async Task<ActionResult<Envelope<ExchangeDto>>> Get([FromQuery] int pageNumber = 1)
    {
        if (pageNumber < 1) pageNumber = 1;
        var envelope = await _exchangeService.GetExchangesAsync(pageNumber);
        return Ok(envelope);
    }
}