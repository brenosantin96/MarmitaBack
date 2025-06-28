public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<Lunchbox> Lunchboxes { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lunchbox>().ToTable("Lunchboxes");
        base.OnModelCreating(modelBuilder);
    }
}
