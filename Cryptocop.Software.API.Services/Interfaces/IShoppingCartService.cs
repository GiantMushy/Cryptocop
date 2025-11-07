using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;

namespace Cryptocop.Software.API.Services.Interfaces;

public interface IShoppingCartService
{
    Task<IEnumerable<ShoppingCartItemDto>> GetCartItems(string email);
    Task AddCartItem(string email, ShoppingCartItemInputModel shoppingCartItemItem);
    Task RemoveCartItem(string email, int id);
    Task UpdateCartItemQuantity(string email, int id, float quantity);
    Task ClearCart(string email);
}