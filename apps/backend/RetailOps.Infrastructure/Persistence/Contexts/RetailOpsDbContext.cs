using Microsoft.EntityFrameworkCore;

namespace RetailOps.Infrastructure.Persistence.Contexts;

public class RetailOpsDbContext : DbContext
{
    public RetailOpsDbContext(DbContextOptions<RetailOpsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RetailOpsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
