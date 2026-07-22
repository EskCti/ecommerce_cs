using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RetailOps.Infrastructure.Persistence.Contexts;

#nullable disable

namespace RetailOps.Infrastructure.Persistence.Migrations;

[DbContext(typeof(RetailOpsDbContext))]
partial class RetailOpsDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.11")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        modelBuilder.Entity("RetailOps.Infrastructure.Persistence.Entities.MigrationCheckpointRow", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("integer");

                b.Property<string>("BoundedContext")
                    .IsRequired()
                    .HasMaxLength(64)
                    .HasColumnType("character varying(64)");

                b.Property<DateTime>("CompletedAtUtc")
                    .HasColumnType("timestamp with time zone");

                b.Property<long>("RowCount")
                    .HasColumnType("bigint");

                b.Property<long>("SampleChecksum")
                    .HasColumnType("bigint");

                b.Property<int>("TenantId")
                    .HasColumnType("integer");

                b.HasKey("Id");

                b.HasIndex("TenantId", "BoundedContext")
                    .IsUnique();

                b.ToTable("migration_checkpoints", (string)null);
            });

        modelBuilder.Entity("RetailOps.Infrastructure.Persistence.Entities.ParallelRunStatRow", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("integer");

                b.Property<decimal>("DivergencePercent")
                    .HasColumnType("numeric");

                b.Property<DateTime>("RecordedAtUtc")
                    .HasColumnType("timestamp with time zone");

                b.Property<int>("TenantId")
                    .HasColumnType("integer");

                b.HasKey("Id");

                b.HasIndex("RecordedAtUtc");

                b.ToTable("migration_parallel_run_stats", (string)null);
            });
#pragma warning restore 612, 618
    }
}
