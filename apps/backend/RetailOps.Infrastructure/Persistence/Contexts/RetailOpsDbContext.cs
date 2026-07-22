using Microsoft.EntityFrameworkCore;
using RetailOps.Infrastructure.Persistence.Entities;

namespace RetailOps.Infrastructure.Persistence.Contexts;

public class RetailOpsDbContext : DbContext
{
    public RetailOpsDbContext(DbContextOptions<RetailOpsDbContext> options)
        : base(options)
    {
    }

    public DbSet<MigrationCheckpointRow> MigrationCheckpoints => Set<MigrationCheckpointRow>();
    public DbSet<ParallelRunStatRow> ParallelRunStats => Set<ParallelRunStatRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RetailOpsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
