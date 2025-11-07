namespace Cryptocop.Software.API.Models;

public class Envelope<T> where T : class
{
    public int PageNumber { get; set; }
    public IEnumerable<T> Items { get; set; }
}