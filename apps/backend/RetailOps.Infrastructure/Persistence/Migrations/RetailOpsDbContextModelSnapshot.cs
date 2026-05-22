using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RetailOps.Infrastructure.Persistence.Contexts;

#nullable disable

namespace RetailOps.Infrastructure.Persistence.Migrations;

[DbContext(typeof(RetailOpsDbContext))]
partial class RetailOpsDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder.HasAnnotation("ProductVersion", "8.0.11");
#pragma warning restore 612, 618
    }
}
