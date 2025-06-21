using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users { get; set; }
    public DbSet<SensorType> SensorTypes { get; set; }
    public DbSet<Sensor> Sensors { get; set; }
    public DbSet<SensorReading> SensorReadings { get; set; }
    public DbSet<Plant> Plants { get; set; }
    public DbSet<Alert> Alerts { get; set; } // Assuming you have an Alert model

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("PBL_IOT");

        modelBuilder.Entity<User>()
            .ToTable("Users", "PBL_IOT")
            .HasKey(u => u.Id);

        modelBuilder.Entity<SensorType>()
            .ToTable("SensorTypes", "PBL_IOT")
            .HasKey(st => st.Id);

        modelBuilder.Entity<Sensor>()
            .ToTable("Sensors", "PBL_IOT")
            .HasKey(s => s.Id);

        modelBuilder.Entity<SensorReading>()
            .ToTable("SensorReadings", "PBL_IOT")
            .HasKey(sr => sr.Id);

        modelBuilder.Entity<Plant>()
            .ToTable("Plants", "PBL_IOT")
            .HasKey(p => p.Id);
        // Ensure DateTime is handled as UTC for PostgreSQL
        modelBuilder.Entity<SensorReading>()
            .Property(sr => sr.Timestamp)
            .HasColumnType("timestamp with time zone");

        modelBuilder.Entity<Alert>()
            .ToTable("Alerts", "PBL_IOT")
            .HasKey(a => a.Id);
    }

}