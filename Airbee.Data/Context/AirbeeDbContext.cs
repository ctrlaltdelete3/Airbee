using Airbee.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Airbee.Data.Context;

public class AirbeeDbContext(DbContextOptions<AirbeeDbContext> options) : DbContext(options)
{
    public DbSet<Sensor> Sensors { get; set; }
    public DbSet<Measurement> Measurements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Sensor>(sensor =>
        {
            sensor.Property(s => s.Id).ValueGeneratedNever();
            sensor.Property(s => s.Type).HasMaxLength(50);
            sensor.Property(s => s.City).HasMaxLength(100);
            // The dashboard filters sensors by city
            sensor.HasIndex(s => s.City);
        });

        modelBuilder.Entity<Measurement>(measurement =>
        {
            measurement.HasOne(m => m.Sensor)
                .WithMany()
                .HasForeignKey(m => m.SensorId);

            // One measurement per sensor per timestamp; also blocks duplicates if a message is processed twice
            measurement.HasIndex(m => new { m.SensorId, m.TimestampUtc })
                .IsUnique();
        });
    }
}
