using HandmadeDolls.Models;
using Microsoft.EntityFrameworkCore;

namespace HandmadeDolls.Data;

public class MinimalContextDb : DbContext
{
    public MinimalContextDb(DbContextOptions<MinimalContextDb> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<DollAcessory> DollsAccessories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderList> OrderLists { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Stock).IsRequired();
            entity.Property(e => e.Active).IsRequired();
            entity.Property(e => e.IsGift).IsRequired();
        });

        modelBuilder.Entity<DollAcessory>(entity =>
        {
            entity.ToTable("DollsAccessories");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.ProductDoll).WithMany(x => x.Dolls).HasForeignKey(e => e.DollId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.ProductAcessory).WithMany(x => x.Acessories).HasForeignKey(e => e.AccessoryId).OnDelete(DeleteBehavior.NoAction);
            entity.Property<DateTime>("RegisteredOn").HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderDate).HasDefaultValueSql("GETDATE()");
     
        });

        modelBuilder.Entity<OrderList>(entity =>
        {
            entity.ToTable("OrderLists");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.Price).IsRequired();
            entity.HasOne(e => e.Order).WithMany(x => x.OrderLists).HasForeignKey(p => p.OrderId).IsRequired().OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Product).WithMany(x => x.OrderLists).HasForeignKey(p => p.ProductId).IsRequired().OnDelete(DeleteBehavior.NoAction);
        });

        base.OnModelCreating(modelBuilder);
    }    
}
