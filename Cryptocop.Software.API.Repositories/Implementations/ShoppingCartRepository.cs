using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Repositories.Interfaces;
using Cryptocop.Software.API.Repositories.Data;
using Microsoft.EntityFrameworkCore;

namespace Cryptocop.Software.API.Repositories.Implementations;

public class ShoppingCartRepository : IShoppingCartRepository
{
    private readonly CryptocopDbContext _db;
    public ShoppingCartRepository(CryptocopDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ShoppingCartItemDto>> GetCartItemsAsync(string email)
    {
        var userId = await _db.Users.Where(u => u.Email == email).Select(u => u.Id).FirstOrDefaultAsync();
        if (userId == 0) return Enumerable.Empty<ShoppingCartItemDto>();

        return await _db.ShoppingCartItems
            .Where(ci => ci.UserId == userId)
            .Select(ci => new ShoppingCartItemDto
            {
                Id = ci.Id,
                ShoppingCartId = 1,
                ProductIdentifier = ci.ProductIdentifier,
                Quantity = ci.Quantity,
                UnitPrice = (float)ci.UnitPrice,
                TotalPrice = (float)ci.TotalPrice
            })
            .ToListAsync();
    }

    public async Task AddCartItemAsync(string email, ShoppingCartItemInputModel shoppingCartItemItem, float priceInUsd)
    {
        var userId = await _db.Users.Where(u => u.Email == email).Select(u => u.Id).FirstOrDefaultAsync();
        if (userId == 0) throw new InvalidOperationException("User not found");
        // Do not add duplicate items; if it exists, no-op
        var identifier = shoppingCartItemItem.ProductIdentifier;
        var existing = await _db.ShoppingCartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductIdentifier == identifier);
        if (existing != null)
        {
            // No changes when already present
            return;
        }
        var qty = 1f; // Always add a single unit
        var unit = (decimal)priceInUsd;
        _db.ShoppingCartItems.Add(new Entities.ShoppingCartItem
        {
            UserId = userId,
            ProductIdentifier = identifier,
            Quantity = qty,
            UnitPrice = unit,
            TotalPrice = unit * (decimal)qty
        });
        await _db.SaveChangesAsync();
    }

    public async Task RemoveCartItemAsync(string email, int id)
    {
        var userId = await _db.Users.Where(u => u.Email == email).Select(u => u.Id).FirstOrDefaultAsync();
        if (userId == 0) return;
        var item = await _db.ShoppingCartItems.FirstOrDefaultAsync(ci => ci.Id == id && ci.UserId == userId);
        if (item != null)
        {
            _db.ShoppingCartItems.Remove(item);
            await _db.SaveChangesAsync();
        }
    }

    public async Task UpdateCartItemQuantityAsync(string email, int id, float quantity)
    {
        var userId = await _db.Users.Where(u => u.Email == email).Select(u => u.Id).FirstOrDefaultAsync();
        if (userId == 0) return;
        var item = await _db.ShoppingCartItems.FirstOrDefaultAsync(ci => ci.Id == id && ci.UserId == userId);
        if (item != null)
        {
            // If quantity is <= 0, remove the item
            if (quantity <= 0f)
            {
                _db.ShoppingCartItems.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
                item.TotalPrice = item.UnitPrice * (decimal)quantity;
            }
            await _db.SaveChangesAsync();
        }
    }

    public async Task ClearCartAsync(string email)
    {
        var userId = await _db.Users.Where(u => u.Email == email).Select(u => u.Id).FirstOrDefaultAsync();
        if (userId == 0) return;
        var items = await _db.ShoppingCartItems.Where(ci => ci.UserId == userId).ToListAsync();
        _db.ShoppingCartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteCartAsync(string email)
    {
        await ClearCartAsync(email);
    }
}