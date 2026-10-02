using Microsoft.EntityFrameworkCore;

namespace WiresAndPipes.Api.Data;

public sealed class WiresAndPipesDbContext(DbContextOptions<WiresAndPipesDbContext> options)
    : DbContext(options)
{
    public DbSet<InterconnectorReading> InterconnectorReadings => Set<InterconnectorReading>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InterconnectorReading>(entity =>
        {
            entity.HasIndex(r => new { r.InterconnectorCode, r.SettlementDate, r.SettlementPeriod })
                .IsUnique();
        });
    }
}
