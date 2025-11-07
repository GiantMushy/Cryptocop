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

    public async Task<IEnumerable<ShoppingCartItemDto>> GetCartItems(string email)
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

    public async Task AddCartItem(string email, ShoppingCartItemInputModel shoppingCartItemItem, float priceInUsd)
    {
        var userId = await _db.Users.Where(u => u.Email == email).Select(u => u.Id).FirstOrDefaultAsync();
        if (userId == 0) throw new InvalidOperationException("User not found");
        var identifier = shoppingCartItemItem.ProductIdentifier;
        // Input model now uses double?; cast to float for persistence layer
        var requestedQty = (float)(shoppingCartItemItem.Quantity ?? 0.01d);
        if (requestedQty <= 0f) requestedQty = 0.01f;
        // Normalize to 2 decimals for storage/display consistency.
        requestedQty = (float)Math.Round(requestedQty, 2, MidpointRounding.AwayFromZero);

        var existing = await _db.ShoppingCartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductIdentifier == identifier);

        var unit = (decimal)priceInUsd;

        if (existing != null)
        {
            // Accumulate quantity if item already exists
            existing.Quantity = (float)Math.Round(existing.Quantity + requestedQty, 2, MidpointRounding.AwayFromZero);
            // Keep original unit price if present; otherwise use latest non-zero price if available
            if (existing.UnitPrice <= 0 && unit > 0) existing.UnitPrice = unit;
            existing.TotalPrice = Math.Round(existing.UnitPrice * (decimal)existing.Quantity, 2, MidpointRounding.AwayFromZero);
        }
        else
        {
            var toAddQty = requestedQty; // already rounded above
            _db.ShoppingCartItems.Add(new Entities.ShoppingCartItem
            {
                UserId = userId,
                ProductIdentifier = identifier,
                Quantity = toAddQty,
                UnitPrice = unit,
                TotalPrice = unit * (decimal)toAddQty
            });
        }
        await _db.SaveChangesAsync();
    }

    public async Task RemoveCartItem(string email, int id)
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

    public async Task UpdateCartItemQuantity(string email, int id, float quantity)
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
                // Normalize quantity to 2 decimals on updates as well
                item.Quantity = (float)Math.Round(quantity, 2, MidpointRounding.AwayFromZero);
                item.TotalPrice = Math.Round(item.UnitPrice * (decimal)item.Quantity, 2, MidpointRounding.AwayFromZero);
            }
            await _db.SaveChangesAsync();
        }
    }

    public async Task ClearCart(string email)
    {
        var userId = await _db.Users.Where(u => u.Email == email).Select(u => u.Id).FirstOrDefaultAsync();
        if (userId == 0) return;
        var items = await _db.ShoppingCartItems.Where(ci => ci.UserId == userId).ToListAsync();
        _db.ShoppingCartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteCart(string email)
    {
        await ClearCart(email);
    }
}