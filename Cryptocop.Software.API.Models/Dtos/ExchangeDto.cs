using Newtonsoft.Json;

namespace Cryptocop.Software.API.Models.Dtos;

public class ExchangeDto
{
	[JsonProperty("id")]
	public string Id { get; set; } = string.Empty;

	[JsonProperty("exchange_name")]
	public string Name { get; set; } = string.Empty;

	[JsonProperty("exchange_slug")]
	public string Slug { get; set; } = string.Empty;

	[JsonProperty("base_asset_symbol")]
	public string AssetSymbol { get; set; } = string.Empty;

	[JsonProperty("price_usd")]
	public float? PriceInUsd { get; set; }

	[JsonProperty("last_trade_at")]
	public DateTime? LastTrade { get; set; }
}