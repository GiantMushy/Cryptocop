namespace Cryptocop.Software.API.Repositories.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<PaymentCard> PaymentCards { get; set; } = new List<PaymentCard>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<ShoppingCartItem> ShoppingCartItems { get; set; } = new List<ShoppingCartItem>();
}
