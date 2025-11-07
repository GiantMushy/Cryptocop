using Newtonsoft.Json;

namespace Cryptocop.Software.API.Models.Dtos;

public class CryptoCurrencyDto
{
	public string Id { get; set; } = string.Empty;
	public string Symbol { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty;
	[JsonProperty("price_usd")] public double? PriceInUsd { get; set; }
}