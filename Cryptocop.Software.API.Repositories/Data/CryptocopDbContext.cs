using Cryptocop.Software.API.Repositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cryptocop.Software.API.Repositories.Data;

public class CryptocopDbContext : DbContext
{
    public CryptocopDbContext(DbContextOptions<CryptocopDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<PaymentCard> PaymentCards => Set<PaymentCard>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<ShoppingCartItem> ShoppingCartItems => Set<ShoppingCartItem>();
    public DbSet<Token> Tokens => Set<Token>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Users
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Address
        modelBuilder.Entity<Address>()
            .HasOne(a => a.User)
            .WithMany(u => u.Addresses)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // PaymentCard
        modelBuilder.Entity<PaymentCard>()
            .HasOne(pc => pc.User)
            .WithMany(u => u.PaymentCards)
            .HasForeignKey(pc => pc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ShoppingCartItem
        modelBuilder.Entity<ShoppingCartItem>()
            .HasOne(sci => sci.User)
            .WithMany(u => u.ShoppingCartItems)
            .HasForeignKey(sci => sci.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Orders
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // OrderItems
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Token
        modelBuilder.Entity<Token>();
    }
}
