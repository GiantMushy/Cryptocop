using System.ComponentModel.DataAnnotations;

namespace Cryptocop.Software.API.Models.InputModels;

public class ShoppingCartItemInputModel
{
	[Required]
	public string ProductIdentifier { get; set; } = string.Empty;

	public int ShoppingCartId { get; set; }

	[Required]
	[Range(0.01, float.MaxValue)]
	public float? Quantity { get; set; }
}