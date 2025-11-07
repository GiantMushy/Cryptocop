using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;

namespace Cryptocop.Software.API.Repositories.Interfaces;

public interface IShoppingCartRepository
{
    Task<IEnumerable<ShoppingCartItemDto>> GetCartItemsAsync(string email);
    Task AddCartItemAsync(string email, ShoppingCartItemInputModel shoppingCartItemItem, float priceInUsd);
    Task RemoveCartItemAsync(string email, int id);
    Task UpdateCartItemQuantityAsync(string email, int id, float quantity);
    Task ClearCartAsync(string email);
    Task DeleteCartAsync(string email);
}