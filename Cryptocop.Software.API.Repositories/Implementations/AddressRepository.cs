using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Repositories.Interfaces;
using Cryptocop.Software.API.Repositories.Data;
using Microsoft.EntityFrameworkCore;

namespace Cryptocop.Software.API.Repositories.Implementations;

public class AddressRepository : IAddressRepository
{
    private readonly CryptocopDbContext _db;

    public AddressRepository(CryptocopDbContext db)
    {
        _db = db;
    }

    public async Task AddAddressAsync(string email, AddressInputModel address)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email)
                   ?? throw new InvalidOperationException("User not found");

        _db.Addresses.Add(new Entities.Address
        {
            UserId = user.Id,
            StreetName = address.StreetName.Trim(),
            HouseNumber = address.HouseNumber.Trim(),
            ZipCode = address.ZipCode.Trim(),
            Country = address.Country.Trim(),
            City = address.City.Trim()
        });
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<AddressDto>> GetAllAddressesAsync(string email)
    {
        var userId = await _db.Users.Where(u => u.Email == email).Select(u => u.Id).FirstOrDefaultAsync();
        if (userId == 0) return Enumerable.Empty<AddressDto>();

        return await _db.Addresses
            .Where(a => a.UserId == userId)
            .Select(a => new AddressDto
            {
                Id = a.Id,
                StreetName = a.StreetName,
                HouseNumber = a.HouseNumber,
                ZipCode = a.ZipCode,
                Country = a.Country,
                City = a.City
            })
            .ToListAsync();
    }

    public async Task DeleteAddressAsync(string email, int addressId)
    {
        var userId = await _db.Users.Where(u => u.Email == email).Select(u => u.Id).FirstOrDefaultAsync();
        if (userId == 0) return;

        var address = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);
        if (address != null)
        {
            _db.Addresses.Remove(address);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<AddressDto?> GetAddressByIdAsync(string email, int addressId)
    {
        var userId = await _db.Users.Where(u => u.Email == email).Select(u => u.Id).FirstOrDefaultAsync();
        if (userId == 0) return null;

        return await _db.Addresses
            .Where(a => a.Id == addressId && a.UserId == userId)
            .Select(a => new AddressDto
            {
                Id = a.Id,
                StreetName = a.StreetName,
                HouseNumber = a.HouseNumber,
                ZipCode = a.ZipCode,
                Country = a.Country,
                City = a.City
            })
            .FirstOrDefaultAsync();
    }
}