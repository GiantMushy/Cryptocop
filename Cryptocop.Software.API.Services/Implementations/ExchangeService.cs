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
        var result = new List<ExchangeDto>();
        var limit = 50;

        // 1) Try Messari v1 markets (may require API key). If successful, map flattened fields.
        try
        {
            var url = $"/api/v1/markets?limit={limit}&page={pageNumber}";
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var flat = await response.DeserializeJsonToList<dynamic>(flatten: true) ?? Enumerable.Empty<dynamic>();
                foreach (var it in flat)
                {
                    try
                    {
                        string id = it.id ?? it.Id ?? string.Empty;
                        string name = it.exchange_name ?? it.name ?? it.Name ?? string.Empty;
                        string slug = it.exchange_slug ?? it.slug ?? it.Slug ?? string.Empty;
                        string assetSymbol = it.base_asset_symbol ?? it.AssetSymbol ?? it.symbol ?? it.Symbol ?? string.Empty;
                        float? price = null;
                        try
                        {
                            var p = (double?) (it.price_usd ?? it.Price_usd ?? it.PriceUsd);
                            if (p.HasValue) price = (float)p.Value;
                        }
                        catch { /* ignore price parse */ }

                        result.Add(new ExchangeDto
                        {
                            Id = id,
                            Name = name,
                            Slug = slug,
                            AssetSymbol = assetSymbol,
                            PriceInUsd = price
                        });
                    }
                    catch { /* ignore row */ }
                }
            }
        }
        catch { /* ignore and fall back */ }

        // 2) Fallback: use assets v2 as a surrogate list so UI isn't empty (no API key required for top assets)
        if (result.Count == 0)
        {
            try
            {
                var url2 = $"/api/v2/assets?limit={limit}&fields=id,slug,symbol,metrics/market_data/price_usd";
                var resp2 = await _httpClient.GetAsync(url2);
                if (resp2.IsSuccessStatusCode)
                {
                    var items = await resp2.DeserializeJsonToList<dynamic>(flatten: true) ?? Enumerable.Empty<dynamic>();
                    foreach (var it in items)
                    {
                        try
                        {
                            string id = it.id ?? it.Id ?? it.slug ?? it.Slug ?? it.symbol ?? it.Symbol ?? Guid.NewGuid().ToString("N");
                            string symbol = it.symbol ?? it.Symbol ?? string.Empty;
                            string slug = it.slug ?? it.Slug ?? string.Empty;
                            float? price = null;
                            try
                            {
                                var p = (double?) (it.price_usd ?? it.Price_usd ?? it.PriceUsd);
                                if (p.HasValue) price = (float)p.Value;
                            }
                            catch { }

                            result.Add(new ExchangeDto
                            {
                                Id = id,
                                Name = symbol,
                                Slug = slug,
                                AssetSymbol = symbol,
                                PriceInUsd = price
                            });
                        }
                        catch { /* ignore row */ }
                    }
                }
            }
            catch { /* give up; return empty */ }
        }

        return new Envelope<ExchangeDto>
        {
            PageNumber = pageNumber,
            Items = result
        };
    }
}