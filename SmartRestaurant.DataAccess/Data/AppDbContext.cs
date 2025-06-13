using Microsoft.EntityFrameworkCore;
using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.DataAccess.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Table> Tables { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<TableCategory> TableCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("product");
            entity.HasKey(p => p.Id);

            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("category");
            entity.HasKey(c => c.Id);
        });

        modelBuilder.Entity<Table>(entity =>
        {
            entity.ToTable("table");
            entity.HasKey(t => t.Id);

            entity.HasOne(t => t.TableCategory)
                  .WithMany(tc => tc.Tables)
                  .HasForeignKey(t => t.TableCategoryId)
                  .OnDelete(DeleteBehavior.SetNull)
                  .IsRequired(false);
        });

        modelBuilder.Entity<TableCategory>(entity =>
        {
            entity.ToTable("table_category");
            entity.HasKey(tc => tc.Id);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("order");
            entity.HasKey(o => o.Id);

            entity.HasOne(o => o.Table)
                  .WithMany(t => t.Orders)
                  .HasForeignKey(o => o.TableId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_item");
            entity.HasKey(oi => oi.Id);

            entity.HasOne(oi => oi.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(oi => oi.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("user");
            entity.HasKey(u => u.Id);

            entity.HasMany<Order>()
                  .WithOne(o => o.OrderedByUser)
                  .HasForeignKey(o => o.OrderedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany<Order>()
                  .WithOne(o => o.ClosedByUser)
                  .HasForeignKey(o => o.ClosedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(u => u.PhoneNumber)
                  .IsUnique();

            entity.Property(u => u.PhoneNumber)
                  .IsRequired(false);
        });
    }
}
