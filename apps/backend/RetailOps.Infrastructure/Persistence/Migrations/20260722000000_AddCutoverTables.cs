using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetailOps.Infrastructure.Persistence.Migrations;

public partial class AddCutoverTables : Microsoft.EntityFrameworkCore.Migrations.Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "migration_checkpoints",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                TenantId = table.Column<int>(type: "integer", nullable: false),
                BoundedContext = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                RowCount = table.Column<long>(type: "bigint", nullable: false),
                SampleChecksum = table.Column<long>(type: "bigint", nullable: false),
                CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_migration_checkpoints", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_migration_checkpoints_TenantId_BoundedContext",
            table: "migration_checkpoints",
            columns: new[] { "TenantId", "BoundedContext" },
            unique: true);

        migrationBuilder.CreateTable(
            name: "migration_parallel_run_stats",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                TenantId = table.Column<int>(type: "integer", nullable: false),
                DivergencePercent = table.Column<decimal>(type: "numeric", nullable: false),
                RecordedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_migration_parallel_run_stats", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_migration_parallel_run_stats_RecordedAtUtc",
            table: "migration_parallel_run_stats",
            column: "RecordedAtUtc");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "migration_checkpoints");
        migrationBuilder.DropTable(name: "migration_parallel_run_stats");
    }
}
