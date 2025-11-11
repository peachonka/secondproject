using Microsoft.EntityFrameworkCore;
using OrderService.Models;

namespace OrderService.Data;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Order>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.UserId);
        entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
        
        // Настройка связи с OrderItem
        entity.OwnsMany(e => e.Items, item =>
        {
            item.WithOwner().HasForeignKey("OrderId");
            item.Property<int>("Id"); // Внутренний ID для OrderItem
            item.HasKey("Id");
        });
    });
}
}