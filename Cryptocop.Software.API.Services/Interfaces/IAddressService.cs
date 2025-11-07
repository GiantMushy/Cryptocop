using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;

namespace Cryptocop.Software.API.Services.Interfaces;

public interface IAddressService
{
    Task AddAddressAsync(string email, AddressInputModel address);
    Task<IEnumerable<AddressDto>> GetAllAddressesAsync(string email);
    Task DeleteAddressAsync(string email, int addressId);
}