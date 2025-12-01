using MarmitaBackend.Models;
using MarmitaBackend.Provider;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantProvider tenantProvider
    )
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    // DbSets
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Lunchbox> Lunchboxes { get; set; }
    public DbSet<Kit> Kits { get; set; }
    public DbSet<KitLunchbox> KitLunchboxes { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<DeliveryInfo> DeliveryInfo { get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===========================
        // GLOBAL QUERY FILTERS
        // ===========================
        // NÃO PEGUE TenantId AQUI DIRETAMENTE
        // O valor precisa ser buscado em tempo de execução
        modelBuilder.Entity<User>().HasQueryFilter(e => e.TenantId == _tenantProvider.TenantId);
        modelBuilder.Entity<Address>().HasQueryFilter(e => e.TenantId == _tenantProvider.TenantId);
        modelBuilder.Entity<Cart>().HasQueryFilter(e => e.TenantId == _tenantProvider.TenantId);
        modelBuilder.Entity<CartItem>().HasQueryFilter(e => e.TenantId == _tenantProvider.TenantId);
        modelBuilder.Entity<Category>().HasQueryFilter(e => e.TenantId == _tenantProvider.TenantId);
        modelBuilder.Entity<Lunchbox>().HasQueryFilter(e => e.TenantId == _tenantProvider.TenantId);
        modelBuilder.Entity<Kit>().HasQueryFilter(e => e.TenantId == _tenantProvider.TenantId);
        modelBuilder.Entity<KitLunchbox>().HasQueryFilter(e => e.TenantId == _tenantProvider.TenantId);
        modelBuilder.Entity<Order>().HasQueryFilter(e => e.TenantId == _tenantProvider.TenantId);
        modelBuilder.Entity<DeliveryInfo>().HasQueryFilter(e => e.TenantId == _tenantProvider.TenantId);
        modelBuilder.Entity<PaymentMethod>().HasQueryFilter(e => e.TenantId == _tenantProvider.TenantId);

        // ===========================
        // UNIQUE EMAIL POR TENANT
        // ===========================
        modelBuilder.Entity<User>()
            .HasIndex(u => new { u.Email, u.TenantId })
            .IsUnique();

        // ===========================
        // RELACIONAMENTO ADDRESS → USER
        // ===========================
        modelBuilder.Entity<Address>()
            .HasOne(a => a.User)
            .WithMany(u => u.Addresses)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===========================
        // PRECISÃO DECIMAL
        // ===========================
        modelBuilder.Entity<Lunchbox>()
            .Property(i => i.Price)
            .HasColumnType("decimal(10,2)");

        modelBuilder.Entity<Kit>()
            .Property(i => i.Price)
            .HasColumnType("decimal(10,2)");
    }
}
