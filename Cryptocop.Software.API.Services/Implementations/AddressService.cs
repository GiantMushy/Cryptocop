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

    public Task AddAddress(string email, AddressInputModel address)
        => _addressRepository.AddAddress(email, address);

    public Task<IEnumerable<AddressDto>> GetAllAddresses(string email)
        => _addressRepository.GetAllAddresses(email);

    public Task DeleteAddress(string email, int addressId)
        => _addressRepository.DeleteAddress(email, addressId);
}