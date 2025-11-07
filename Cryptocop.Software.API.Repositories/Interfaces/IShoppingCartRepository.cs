using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;

namespace Cryptocop.Software.API.Repositories.Interfaces;

public interface IShoppingCartRepository
{
    Task<IEnumerable<ShoppingCartItemDto>> GetCartItems(string email);
    Task AddCartItem(string email, ShoppingCartItemInputModel shoppingCartItemItem, float priceInUsd);
    Task RemoveCartItem(string email, int id);
    Task UpdateCartItemQuantity(string email, int id, float quantity);
    Task ClearCart(string email);
    Task DeleteCart(string email);
}