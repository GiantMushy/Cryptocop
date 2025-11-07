using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;

namespace Cryptocop.Software.API.Repositories.Interfaces;

public interface IAddressRepository
{
    Task AddAddressAsync(string email, AddressInputModel address);
    Task<IEnumerable<AddressDto>> GetAllAddressesAsync(string email);
    Task DeleteAddressAsync(string email, int addressId);
    Task<AddressDto?> GetAddressByIdAsync(string email, int addressId);
}