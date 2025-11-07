using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;

namespace Cryptocop.Software.API.Services.Interfaces;

public interface IAddressService
{
    Task AddAddress(string email, AddressInputModel address);
    Task<IEnumerable<AddressDto>> GetAllAddresses(string email);
    Task DeleteAddress(string email, int addressId);
}