namespace Cryptocop.Software.API.Models.InputModels;

public class AddressInputModel
{
	[System.ComponentModel.DataAnnotations.Required]
	public string StreetName { get; set; } = string.Empty;

	[System.ComponentModel.DataAnnotations.Required]
	public string HouseNumber { get; set; } = string.Empty;

	[System.ComponentModel.DataAnnotations.Required]
	public string ZipCode { get; set; } = string.Empty;

	[System.ComponentModel.DataAnnotations.Required]
	public string Country { get; set; } = string.Empty;

	[System.ComponentModel.DataAnnotations.Required]
	public string City { get; set; } = string.Empty;
}