namespace Cryptocop.Software.API.Repositories.Entities;

public class Address
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string StreetName { get; set; } = string.Empty;
    public string HouseNumber { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    public User? User { get; set; }
}
