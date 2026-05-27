using DreamsPools.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DreamsPools.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Tables
    public DbSet<User> Users => Set<User>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();
    public DbSet<Banner> Banners => Set<Banner>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global filter - soft delete
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Agent>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Order>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Appointment>().HasQueryFilter(e => !e.IsDeleted);

        // Decimal precision
        modelBuilder.Entity<Product>()
            .Property(p => p.Price).HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.SubTotal).HasPrecision(18, 2);
        modelBuilder.Entity<Order>()
            .Property(o => o.VatAmount).HasPrecision(18, 2);
        modelBuilder.Entity<Order>()
            .Property(o => o.DeliveryFee).HasPrecision(18, 2);
        modelBuilder.Entity<Order>()
            .Property(o => o.DiscountAmount).HasPrecision(18, 2);
        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount).HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(o => o.UnitPrice).HasPrecision(18, 2);
        modelBuilder.Entity<OrderItem>()
            .Property(o => o.VatAmount).HasPrecision(18, 2);
        modelBuilder.Entity<OrderItem>()
            .Property(o => o.TotalPrice).HasPrecision(18, 2);

        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<Transaction>()
            .Property(t => t.VatAmount).HasPrecision(18, 2);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.SubTotal).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>()
            .Property(i => i.VatAmount).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>()
            .Property(i => i.TotalAmount).HasPrecision(18, 2);

        modelBuilder.Entity<Expense>()
            .Property(e => e.Amount).HasPrecision(18, 2);

        modelBuilder.Entity<Coupon>()
            .Property(c => c.DiscountValue).HasPrecision(18, 2);
        modelBuilder.Entity<Coupon>()
            .Property(c => c.MinOrderAmount).HasPrecision(18, 2);
        modelBuilder.Entity<Coupon>()
            .Property(c => c.MaxDiscountAmount).HasPrecision(18, 2);

        // Unique indexes
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Agent>()
            .HasIndex(a => a.Email).IsUnique();
        modelBuilder.Entity<Admin>()
            .HasIndex(a => a.Email).IsUnique();
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderNumber).IsUnique();
        modelBuilder.Entity<Appointment>()
            .HasIndex(a => a.AppointmentNumber).IsUnique();
        modelBuilder.Entity<Invoice>()
            .HasIndex(i => i.InvoiceNumber).IsUnique();
        modelBuilder.Entity<Coupon>()
            .HasIndex(c => c.Code).IsUnique();
        modelBuilder.Entity<AppSettings>()
            .HasIndex(s => s.Key).IsUnique();
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Agent)
            .WithMany()
            .HasForeignKey(o => o.AgentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Rating>()
            .HasOne(r => r.Agent)
            .WithMany(a => a.Ratings)
            .HasForeignKey(r => r.AgentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Rating>()
            .HasOne(r => r.Product)
            .WithMany(p => p.Ratings)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Agent)
            .WithMany()
            .HasForeignKey(t => t.AgentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Seed default admin
        modelBuilder.Entity<Admin>().HasData(new Admin
        {
            Id = 1,
            FullName = "Super Admin",
            Email = "admin@dreamspools.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            PhoneNumber = "0500000000",
            Role = AdminRole.SuperAdmin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        // Seed default app settings
        modelBuilder.Entity<AppSettings>().HasData(
            new AppSettings { Id = 1, Key = "VatPercentage", Value = "15", Description = "نسبة ضريبة القيمة المضافة", CreatedAt = DateTime.UtcNow },
            new AppSettings { Id = 2, Key = "DeliveryFee", Value = "20", Description = "رسوم التوصيل الثابتة بالريال", CreatedAt = DateTime.UtcNow },
            new AppSettings { Id = 3, Key = "AgentCommissionPercentage", Value = "10", Description = "نسبة عمولة المندوب من قيمة الطلب", CreatedAt = DateTime.UtcNow },
            new AppSettings { Id = 4, Key = "AppName", Value = "DreamsPools", Description = "اسم التطبيق", CreatedAt = DateTime.UtcNow }
        );
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-update timestamps
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}
