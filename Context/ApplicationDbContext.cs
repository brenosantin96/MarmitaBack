using Microsoft.EntityFrameworkCore; // 👈 ESSENCIAL!
using MarmitaBackend.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<User> Users { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Lunchbox> Lunchboxes { get; set; }
    public DbSet<Kit> Kits { get; set; }
    public DbSet<KitLunchbox> KitLunchboxes { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Address>()
    .HasOne(a => a.User)
    .WithMany(u => u.Addresses)
    .HasForeignKey(a => a.UserId);



    }

    public DbSet<MarmitaBackend.Models.DeliveryInfo> DeliveryInfo { get; set; } = default!;



}
