using System.Net.Http;
using System.Linq;
using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Services.Helpers;
using Cryptocop.Software.API.Services.Interfaces;
using Newtonsoft.Json;

namespace Cryptocop.Software.API.Services.Implementations;

public class CryptoCurrencyService : ICryptoCurrencyService
{
    private readonly HttpClient _httpClient;

    public CryptoCurrencyService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<CryptoCurrencyDto>> GetAvailableCryptocurrenciesAsync()
    {
        // v2 assets with price; flatten so price_usd maps to DTO's PriceInUsd
        var url = "/api/v2/assets?limit=50&fields=id,slug,symbol,name,metrics/market_data/price_usd";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var list = (await response.DeserializeJsonToList<CryptoCurrencyDto>(flatten: true))?.ToList() ?? new List<CryptoCurrencyDto>();

        // Ensure key assets are present: BTC, ETH, USDT, LINK.
        var requiredSlugs = new[] { "bitcoin", "ethereum", "tether", "chainlink" };
        var haveSlugs = new HashSet<string>(list
            .Select(x => x.Slug)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.ToLowerInvariant()));

        var missingSlugs = requiredSlugs.Where(s => !haveSlugs.Contains(s)).ToArray();
        if (missingSlugs.Length > 0)
        {
            var slugParam = string.Join(',', missingSlugs);
            var urlEnsure = $"/api/v2/assets?slugs={slugParam}&fields=id,slug,symbol,name,metrics/market_data/price_usd";
            var respEnsure = await _httpClient.GetAsync(urlEnsure);
            if (respEnsure.IsSuccessStatusCode)
            {
                var ensureItems = await respEnsure.DeserializeJsonToList<CryptoCurrencyDto>(flatten: true) ?? Enumerable.Empty<CryptoCurrencyDto>();
                foreach (var item in ensureItems)
                {
                    if (item == null) continue;
                    var duplicate = list.Any(x =>
                        (!string.IsNullOrEmpty(x.Slug) && !string.IsNullOrEmpty(item.Slug) && string.Equals(x.Slug, item.Slug, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(x.Id) && !string.IsNullOrEmpty(item.Id) && string.Equals(x.Id, item.Id, StringComparison.OrdinalIgnoreCase))
                    );
                    if (!duplicate)
                    {
                        list.Add(item);
                    }
                }
            }
        }

        return list;
    }

    public async Task<double?> GetPriceUsdAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier)) return null;
        try
        {
            var normalized = identifier.Trim().ToLowerInvariant();
            var key = Uri.EscapeDataString(normalized);

            // Use v2 assets filter by slug to fetch price directly (no API key required)
            var urlListBySlug = $"/api/v2/assets?slugs={key}&limit=1&fields=metrics/market_data/price_usd";
            var responseList = await _httpClient.GetAsync(urlListBySlug);
            if (responseList.IsSuccessStatusCode)
            {
                var list = await responseList.DeserializeJsonToList<PriceResponse>(flatten: true);
                var p = list.FirstOrDefault()?.PriceUsd;
                if ((p ?? 0d) > 0) return p;
            }

            // As a broader safety net, query a larger page and match by slug/symbol
            var urlTop = "/api/v2/assets?limit=500&fields=slug,symbol,metrics/market_data/price_usd";
            var responseTop = await _httpClient.GetAsync(urlTop);
            if (responseTop.IsSuccessStatusCode)
            {
                var items = await responseTop.DeserializeJsonToList<dynamic>(flatten: true);
                foreach (var it in items)
                {
                    try
                    {
                        string slug = it.slug ?? it.Slug ?? string.Empty;
                        string symbol = it.symbol ?? it.Symbol ?? string.Empty;
                        double p = (double)(it.price_usd ?? it.Price_usd ?? it.PriceUsd ?? 0d);
                        if (!string.IsNullOrEmpty(slug) && slug.ToString().ToLowerInvariant() == normalized && p > 0) return p;
                        if (!string.IsNullOrEmpty(symbol) && symbol.ToString().ToLowerInvariant() == normalized && p > 0) return p;
                    }
                    catch { /* ignore row */ }
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private class PriceResponse
    {
        [JsonProperty("price_usd")] public double PriceUsd { get; set; }
    }
}