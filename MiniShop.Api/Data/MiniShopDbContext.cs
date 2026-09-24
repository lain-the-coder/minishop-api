using Microsoft.EntityFrameworkCore;
using MiniShop.Api.Entities;

namespace MiniShop.Api.Data;

public class MiniShopDbContext(DbContextOptions<MiniShopDbContext> options)
    : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Product>(p =>
        {
            p.Property(x => x.Name).HasMaxLength(200);
            p.Property(x => x.Price).HasPrecision(18, 2);
            p.Property(x => x.Description).HasMaxLength(2000);
            // Seed 3 products (varied price/stock, one with description, one null)
            p.HasData(
                new { Id = 1, Name = "Coffee Mug", Price = 14.99m, Stock = 120, Description = (string?)"High-grade ceramic mug" },
                new { Id = 2, Name = "Mechanical Keyboard", Price = 89.50m, Stock = 45, Description = (string?)"Tenkeyless mechanical keyboard" },
                new { Id = 3, Name = "Mousepad", Price = 9.99m, Stock = 200, Description = (string?)null }
            );
        });

        modelBuilder.Entity<User>(u =>
        {
            u.Property(x => x.Email).HasMaxLength(256);
            u.HasIndex(x => x.Email).IsUnique();
            // Seed 2 users matching dev token identities
            u.HasData(
                new { Id = 1, Email = "alice@minishop.test" },
                new { Id = 2, Email = "bob@minishop.test" }
            );
        });

        modelBuilder.Entity<Order>(o =>
        {
            o.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            o.HasOne(x => x.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(oi =>
        {
            oi.Property(x => x.PriceAtPurchase).HasPrecision(18, 2);

            oi.HasOne(x => x.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            oi.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}