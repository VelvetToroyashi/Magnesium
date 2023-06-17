using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;

namespace Magnesium.Data;

public class MagnesiumContext : DbContext 
{
    public DbSet<TrackedChannel> Channels { get; set; }
    public DbSet<UserPreference> Preferences { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("FileName=Magnesium.db");
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserPreference>().HasMany(p => p.PersonallyTrackedChannels).WithOne(t => t.TrackingUserPreference);

        base.OnModelCreating(modelBuilder);
    }
}