using HandmadeDolls.Models;
using Microsoft.EntityFrameworkCore;

namespace HandmadeDolls.Data;

public class MinimalContextDb : DbContext
{
    public MinimalContextDb(DbContextOptions<MinimalContextDb> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Doll> Dolls { get; set; }
    public DbSet<Accessory> Accessories { get; set; }
    public DbSet<DollAcessory> DollsAccessories { get; set; }
    public DbSet<Status> Status { get; set; }
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
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.Active).IsRequired();
        });

        modelBuilder.Entity<Doll>().ToTable("Dolls");

        modelBuilder.Entity<Accessory>().ToTable("Accessories");

        modelBuilder.Entity<DollAcessory>(entity =>
        {
            entity.ToTable("DollsAccessories");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Doll).WithMany(x => x.DollsAccessories).HasForeignKey(e => e.DollId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Accessory).WithMany(x => x.DollsAccessories).HasForeignKey(e => e.AccessoryId).OnDelete(DeleteBehavior.NoAction);
            entity.Property<DateTime>("RegisteredOn").HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<Status>( entity => 
        {
            entity.ToTable("Status");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(20);
            entity.HasData(
                new Status { Id = 1, Name = "Order Received" }, 
                new Status { Id = 2, Name = "Awaiting Payment" },
                new Status { Id = 3, Name = "Order in Separation" },
                new Status { Id = 4, Name = "Invoice Issued" },
                new Status { Id = 5, Name = "Order Delivered" }
            );
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderDate).HasDefaultValueSql("GETDATE()");
            entity.HasOne(e => e.Status).WithMany(x => x.Orders).HasForeignKey(p => p.StatusId).IsRequired().OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<OrderList>(entity =>
        {
            entity.ToTable("OrderLists");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Order).WithMany(x => x.OrderLists).HasForeignKey(p => p.OrderId).IsRequired().OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Product).WithMany(x => x.OrderLists).HasForeignKey(p => p.ProductId).IsRequired().OnDelete(DeleteBehavior.NoAction);
        });

        base.OnModelCreating(modelBuilder);
    }    
}
