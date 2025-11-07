namespace Cryptocop.Software.API.Repositories.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string ProductIdentifier { get; set; } = string.Empty;
    public float Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    public Order? Order { get; set; }
}
