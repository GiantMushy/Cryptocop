namespace Cryptocop.Software.API.Models.InputModels;

public class PaymentCardInputModel
{
	[System.ComponentModel.DataAnnotations.Required]
	[System.ComponentModel.DataAnnotations.MinLength(3)]
	public string CardholderName { get; set; } = string.Empty;

	[System.ComponentModel.DataAnnotations.Required]
	[System.ComponentModel.DataAnnotations.CreditCard]
	public string CardNumber { get; set; } = string.Empty;

	[System.ComponentModel.DataAnnotations.Range(1, 12)]
	public int Month { get; set; }

	[System.ComponentModel.DataAnnotations.Range(0, 99)]
	public int Year { get; set; }
}