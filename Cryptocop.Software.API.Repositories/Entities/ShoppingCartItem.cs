namespace Cryptocop.Software.API.Repositories.Entities;

public class ShoppingCartItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ProductIdentifier { get; set; } = string.Empty;
    public float Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    public User? User { get; set; }
}
