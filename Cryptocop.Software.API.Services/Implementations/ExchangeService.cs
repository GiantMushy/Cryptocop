using System.Net.Http;
using System.Linq;
using Cryptocop.Software.API.Models;
using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Services.Helpers;
using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class ExchangeService : IExchangeService
{
    private readonly HttpClient _httpClient;

    public ExchangeService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Envelope<ExchangeDto>> GetExchanges(int pageNumber = 1)
    {
        if (pageNumber < 1) pageNumber = 1;
        const int limit = 50;

    // Using instructor-provided mock base (configured in Program.cs) for markets listing
    var url = $"/api/v1/markets?limit={limit}&page={pageNumber}";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            // Propagate a clean error picked up by ProblemDetails middleware
            var status = (int)response.StatusCode;
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Messari markets request failed (status {status}). Body: {body.TruncateForLog(500)}");
        }

        var flat = await response.DeserializeJsonToList<dynamic>(flatten: true) ?? Enumerable.Empty<dynamic>();
        var items = new List<ExchangeDto>();

        foreach (var it in flat)
        {
            // /api/v1/markets returns market rows with exchange + base asset + price
            string id = it.id ?? it.Id ?? string.Empty;
            string name = it.exchange_name ?? it.name ?? it.Name ?? string.Empty;
            string slug = it.exchange_slug ?? it.slug ?? it.Slug ?? string.Empty;
            string assetSymbol = it.base_asset_symbol ?? it.base_symbol ?? it.symbol ?? it.Symbol ?? string.Empty;
            float? price = null;
            try
            {
                var p = (double?) (it.price_usd ?? it.Price_usd ?? it.PriceUsd);
                if (p.HasValue) price = (float)p.Value;
            }
            catch { /* ignore price parse */ }
            DateTime? lastTrade = null;
            try
            {
                lastTrade = (DateTime?) (it.last_trade_at ?? it.Last_trade_at ?? it.LastTradeAt);
            }
            catch { /* ignore date parse */ }

            if (string.IsNullOrWhiteSpace(id) && string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(assetSymbol))
            {
                continue; // skip empty row
            }

            items.Add(new ExchangeDto
            {
                Id = id,
                Name = name,
                Slug = slug,
                AssetSymbol = assetSymbol,
                PriceInUsd = price,
                LastTrade = lastTrade
            });
        }

        return new Envelope<ExchangeDto>
        {
            PageNumber = pageNumber,
            Items = items
        };
    }
}

internal static class ExchangeServiceLogHelpers
{
    // Helper to ensure we don't log excessively large upstream bodies in exception messages
    public static string TruncateForLog(this string? value, int max)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Length <= max ? value : value.Substring(0, max) + "...";
    }
}