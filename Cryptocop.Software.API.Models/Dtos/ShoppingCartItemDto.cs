namespace Cryptocop.Software.API.Models.Dtos;

public class ShoppingCartItemDto
{
	public int Id { get; set; }
	public int ShoppingCartId { get; set; }
	public string ProductIdentifier { get; set; } = string.Empty;
	public float Quantity { get; set; }
	public float UnitPrice { get; set; }
	public float TotalPrice { get; set; }
}