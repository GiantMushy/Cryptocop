using System;

namespace Cryptocop.Software.API.Repositories.Entities;

public class Token
{
    public int Id { get; set; }
    public bool IsBlacklisted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
