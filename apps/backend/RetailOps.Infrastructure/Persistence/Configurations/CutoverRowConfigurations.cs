using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailOps.Infrastructure.Persistence.Entities;

namespace RetailOps.Infrastructure.Persistence.Configurations;

internal sealed class MigrationCheckpointRowConfiguration : IEntityTypeConfiguration<MigrationCheckpointRow>
{
    public void Configure(EntityTypeBuilder<MigrationCheckpointRow> entity)
    {
        entity.ToTable("migration_checkpoints");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).UseIdentityByDefaultColumn();
        entity.Property(e => e.BoundedContext).HasMaxLength(64).IsRequired();
        entity.HasIndex(e => new { e.TenantId, e.BoundedContext }).IsUnique();
    }
}

internal sealed class ParallelRunStatRowConfiguration : IEntityTypeConfiguration<ParallelRunStatRow>
{
    public void Configure(EntityTypeBuilder<ParallelRunStatRow> entity)
    {
        entity.ToTable("migration_parallel_run_stats");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).UseIdentityByDefaultColumn();
        entity.HasIndex(e => e.RecordedAtUtc);
    }
}
