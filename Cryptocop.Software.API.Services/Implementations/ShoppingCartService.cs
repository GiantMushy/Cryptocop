using System.Net.Http;
using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Repositories.Interfaces;
using Cryptocop.Software.API.Services.Helpers;
using Cryptocop.Software.API.Services.Interfaces;
using Newtonsoft.Json;

namespace Cryptocop.Software.API.Services.Implementations;

public class ShoppingCartService : IShoppingCartService
{
    private readonly HttpClient _httpClient;
    private readonly IShoppingCartRepository _repo;

    public ShoppingCartService(HttpClient httpClient, IShoppingCartRepository repo)
    {
        _httpClient = httpClient;
        _repo = repo;
    }

    public Task<IEnumerable<ShoppingCartItemDto>> GetCartItems(string email)
        => _repo.GetCartItems(email);

    public async Task AddCartItem(string email, ShoppingCartItemInputModel shoppingCartItemItem)
    {
        // Fetch current USD price for the product identifier from Messari
        var identifier = shoppingCartItemItem.ProductIdentifier.Trim();
        double priceUsd = 0d;
        var normalized = identifier.Trim().ToLowerInvariant();
        var key = Uri.EscapeDataString(normalized);
        // Use v2 assets filtered by slug and explicitly match by slug/symbol to avoid picking the first (e.g., BTC)
        var urlListBySlug = $"/api/v2/assets?slugs={key}&limit=5&fields=slug,symbol,metrics/market_data/price_usd";
        var responseList = await _httpClient.GetAsync(urlListBySlug);
        if (responseList.IsSuccessStatusCode)
        {
            var list = await responseList.DeserializeJsonToList<AssetPriceRow>(flatten: true);
            var match = list.FirstOrDefault(it =>
                (!string.IsNullOrEmpty(it.Slug) && it.Slug.Equals(normalized, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(it.Symbol) && it.Symbol.Equals(normalized, StringComparison.OrdinalIgnoreCase))
            );
            if (match != null && match.PriceUsd > 0) priceUsd = match.PriceUsd;
        }

    await _repo.AddCartItem(email, shoppingCartItemItem, (float)priceUsd);
    }

    public Task RemoveCartItem(string email, int id)
        => _repo.RemoveCartItem(email, id);

    public Task UpdateCartItemQuantity(string email, int id, float quantity)
        => _repo.UpdateCartItemQuantity(email, id, quantity);

    public Task ClearCart(string email)
        => _repo.ClearCart(email);

    private class PriceResponse
    {
        [JsonProperty("price_usd")]
        public double PriceUsd { get; set; }
    }

    private class AssetPriceRow
    {
        [JsonProperty("slug")] public string? Slug { get; set; }
        [JsonProperty("symbol")] public string? Symbol { get; set; }
        [JsonProperty("price_usd")] public double PriceUsd { get; set; }
    }
}
