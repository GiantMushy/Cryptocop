namespace Cryptocop.Software.API.Models.Dtos;

public class PaymentCardDto
{
	public int Id { get; set; }
	public string CardholderName { get; set; } = string.Empty;
	public string CardNumber { get; set; } = string.Empty;
	public int Month { get; set; }
	public int Year { get; set; }
}