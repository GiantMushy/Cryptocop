using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;

namespace Cryptocop.Software.API.Repositories.Interfaces;

public interface IAddressRepository
{
    Task AddAddress(string email, AddressInputModel address);
    Task<IEnumerable<AddressDto>> GetAllAddresses(string email);
    Task DeleteAddress(string email, int addressId);
    Task<AddressDto?> GetAddressById(string email, int addressId);
}