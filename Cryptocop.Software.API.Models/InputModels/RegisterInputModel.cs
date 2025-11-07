using System.ComponentModel.DataAnnotations;

namespace Cryptocop.Software.API.Models.InputModels;

public class RegisterInputModel
{
	[Required]
	[EmailAddress]
	public string Email { get; set; } = string.Empty;

	[Required]
	[MinLength(3)]
	public string FullName { get; set; } = string.Empty;

	[Required]
	[MinLength(8)]
	public string Password { get; set; } = string.Empty;

	[Required]
	[MinLength(8)]
	[Compare("Password")]
	public string PasswordConfirmation { get; set; } = string.Empty;
}