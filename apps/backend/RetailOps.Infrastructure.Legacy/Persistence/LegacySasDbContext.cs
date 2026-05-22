using Microsoft.EntityFrameworkCore;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;

namespace RetailOps.Infrastructure.Legacy.Persistence;

public sealed class LegacySasDbContext : DbContext
{
    public LegacySasDbContext(DbContextOptions<LegacySasDbContext> options)
        : base(options)
    {
    }

    public DbSet<LegacyCompanyRow> Companies => Set<LegacyCompanyRow>();
    public DbSet<LegacyUserRow> Users => Set<LegacyUserRow>();
    public DbSet<LegacyUserPermissionRow> UserPermissions => Set<LegacyUserPermissionRow>();
    public DbSet<LegacyAccessRow> AccessCatalog => Set<LegacyAccessRow>();
    public DbSet<LegacyProductRow> Products => Set<LegacyProductRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LegacyCompanyRow>(entity =>
        {
            entity.ToTable("empresas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("nome").HasMaxLength(50);
            entity.Property(e => e.Active).HasColumnName("ativo").HasMaxLength(3);
        });

        modelBuilder.Entity<LegacyUserRow>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CompanyId).HasColumnName("empresa");
            entity.Property(e => e.Name).HasColumnName("nome").HasMaxLength(50);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(50);
            entity.Property(e => e.Cpf).HasColumnName("cpf").HasMaxLength(20);
            entity.Property(e => e.PasswordMd5).HasColumnName("senha_crip").HasMaxLength(50);
            entity.Property(e => e.ManagerPinPlain).HasColumnName("senha").HasMaxLength(50);
            entity.Property(e => e.Level).HasColumnName("nivel").HasMaxLength(50);
            entity.Property(e => e.Active).HasColumnName("ativo").HasMaxLength(3);
        });

        modelBuilder.Entity<LegacyUserPermissionRow>(entity =>
        {
            entity.ToTable("usuarios_permissoes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("usuario");
            entity.Property(e => e.AccessId).HasColumnName("permissao");
        });

        modelBuilder.Entity<LegacyAccessRow>(entity =>
        {
            entity.ToTable("acessos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Key).HasColumnName("chave").HasMaxLength(50);
            entity.Property(e => e.Name).HasColumnName("nome").HasMaxLength(100);
            entity.Property(e => e.GroupId).HasColumnName("grupo");
        });

        modelBuilder.Entity<LegacyProductRow>(entity =>
        {
            entity.ToTable("produtos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CompanyId).HasColumnName("empresa");
            entity.Property(e => e.Code).HasColumnName("codigo").HasMaxLength(50);
            entity.Property(e => e.Name).HasColumnName("nome").HasMaxLength(100);
        });

        base.OnModelCreating(modelBuilder);
    }
}
