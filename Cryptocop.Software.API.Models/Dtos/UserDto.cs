namespace Cryptocop.Software.API.Models.Dtos;

public class UserDto
{
	public int Id { get; set; }
	public string FullName { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public int TokenId { get; set; }
}