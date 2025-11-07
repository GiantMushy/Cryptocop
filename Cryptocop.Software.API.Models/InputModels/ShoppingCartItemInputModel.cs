using System.ComponentModel.DataAnnotations;

namespace Cryptocop.Software.API.Models.InputModels;

public class ShoppingCartItemInputModel
{
	[Required]
	public string ProductIdentifier { get; set; } = string.Empty;

	public int ShoppingCartId { get; set; }

	[Required]
	[Range(0.01, double.MaxValue)]
	public double? Quantity { get; set; }
}