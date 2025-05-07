using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("PBL_IOT");

        modelBuilder.Entity<User>()
            .ToTable("Users", "PBL_IOT")
            .HasKey(u => u.Id);
    }

}