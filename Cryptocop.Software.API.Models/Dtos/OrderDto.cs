namespace Cryptocop.Software.API.Models.Dtos;

public class OrderDto
{
	public int Id { get; set; }
	public string Email { get; set; } = string.Empty;
	public string FullName { get; set; } = string.Empty;
	public string StreetName { get; set; } = string.Empty;
	public string HouseNumber { get; set; } = string.Empty;
	public string ZipCode { get; set; } = string.Empty;
	public string Country { get; set; } = string.Empty;
	public string City { get; set; } = string.Empty;
	public string CardholderName { get; set; } = string.Empty;
	public string CreditCard { get; set; } = string.Empty;
	// Represent date as dd.MM.yyyy (e.g., 01.01.2020)
	public string OrderDate { get; set; } = string.Empty;
	public float TotalPrice { get; set; }
	public IEnumerable<OrderItemDto> OrderItems { get; set; } = Array.Empty<OrderItemDto>();
}