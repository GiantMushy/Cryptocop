namespace Cryptocop.Software.API.Models.Dtos;

public class JwtTokenDto
{
	// Token id used internally for blacklisting
	public int Id { get; set; }

	// Serialized JWT returned to clients when signing in
	public string? Token { get; set; }
}