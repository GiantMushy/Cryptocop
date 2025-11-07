using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cryptocop.Software.API.Repositories.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CryptocopDbContext>
{
    public CryptocopDbContext CreateDbContext(string[] args)
    {
        // Prefer explicit --connection passed to dotnet ef; otherwise fall back to env or localhost
        var conn = Environment.GetEnvironmentVariable("ConnectionStrings__CryptocopDb")
                   ?? "Host=localhost;Port=5432;Database=cryptocop;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<CryptocopDbContext>();
        optionsBuilder.UseNpgsql(conn);
        return new CryptocopDbContext(optionsBuilder.Options);
    }
}
