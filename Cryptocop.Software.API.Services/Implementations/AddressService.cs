using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Services.Interfaces;
using Cryptocop.Software.API.Repositories.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class AddressService : IAddressService
{
    private readonly IAddressRepository _addressRepository;

    public AddressService(IAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public Task AddAddressAsync(string email, AddressInputModel address)
        => _addressRepository.AddAddressAsync(email, address);

    public Task<IEnumerable<AddressDto>> GetAllAddressesAsync(string email)
        => _addressRepository.GetAllAddressesAsync(email);

    public Task DeleteAddressAsync(string email, int addressId)
        => _addressRepository.DeleteAddressAsync(email, addressId);
}