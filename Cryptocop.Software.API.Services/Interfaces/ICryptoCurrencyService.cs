using Cryptocop.Software.API.Models.Dtos;

namespace Cryptocop.Software.API.Services.Interfaces;

public interface ICryptoCurrencyService
{
    Task<IEnumerable<CryptoCurrencyDto>> GetAvailableCryptocurrenciesAsync();
    Task<double?> GetPriceUsdAsync(string identifier);
}