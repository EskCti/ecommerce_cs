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
    public DbSet<LegacyContractRow> Contracts => Set<LegacyContractRow>();
    public DbSet<LegacyConfigRow> Configs => Set<LegacyConfigRow>();
    public DbSet<LegacyPaymentMethodRow> PaymentMethods => Set<LegacyPaymentMethodRow>();
    public DbSet<LegacyCashRegisterRow> CashRegisters => Set<LegacyCashRegisterRow>();
    public DbSet<LegacyReceivableRow> Receivables => Set<LegacyReceivableRow>();
    public DbSet<LegacyCashSessionRow> CashSessions => Set<LegacyCashSessionRow>();
    public DbSet<LegacyCartItemRow> CartItems => Set<LegacyCartItemRow>();
    public DbSet<LegacyWithdrawalRow> Withdrawals => Set<LegacyWithdrawalRow>();
    public DbSet<LegacyCustomerRow> Customers => Set<LegacyCustomerRow>();
    public DbSet<LegacySupplierRow> Suppliers => Set<LegacySupplierRow>();
    public DbSet<LegacyAttachmentRow> Attachments => Set<LegacyAttachmentRow>();
    public DbSet<LegacyCategoryRow> Categories => Set<LegacyCategoryRow>();
    public DbSet<LegacyGradeDimensionRow> GradeDimensions => Set<LegacyGradeDimensionRow>();
    public DbSet<LegacyGradeOptionRow> GradeOptions => Set<LegacyGradeOptionRow>();
    public DbSet<LegacyStockEntryRow> StockEntries => Set<LegacyStockEntryRow>();
    public DbSet<LegacyStockExitRow> StockExits => Set<LegacyStockExitRow>();
    public DbSet<LegacyGradeMovementDetailRow> GradeMovementDetails => Set<LegacyGradeMovementDetailRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LegacyCompanyRow>(entity =>
        {
            entity.ToTable("empresas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.Name).HasColumnName("nome").HasMaxLength(50);
            entity.Property(e => e.Phone).HasColumnName("telefone").HasMaxLength(20);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(50);
            entity.Property(e => e.Cpf).HasColumnName("cpf").HasMaxLength(20);
            entity.Property(e => e.Cnpj).HasColumnName("cnpj").HasMaxLength(20);
            entity.Property(e => e.Active).HasColumnName("ativo").HasMaxLength(3);
            entity.Property(e => e.NextBillingDate).HasColumnName("data_pgto");
            entity.Property(e => e.MonthlyFee).HasColumnName("valor");
            entity.Property(e => e.Trial).HasColumnName("teste").HasMaxLength(3);
        });

        modelBuilder.Entity<LegacyUserRow>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.CompanyId).HasColumnName("empresa");
            entity.Property(e => e.Name).HasColumnName("nome").HasMaxLength(50);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(50);
            entity.Property(e => e.Cpf).HasColumnName("cpf").HasMaxLength(20);
            entity.Property(e => e.PasswordMd5).HasColumnName("senha_crip").HasMaxLength(255);
            entity.Property(e => e.ManagerPinPlain).HasColumnName("senha").HasMaxLength(255);
            entity.Property(e => e.Level).HasColumnName("nivel").HasMaxLength(50);
            entity.Property(e => e.Active).HasColumnName("ativo").HasMaxLength(3);
        });

        modelBuilder.Entity<LegacyUserPermissionRow>(entity =>
        {
            entity.ToTable("usuarios_permissoes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.UserId).HasColumnName("usuario");
            entity.Property(e => e.AccessId).HasColumnName("permissao");
        });

        modelBuilder.Entity<LegacyAccessRow>(entity =>
        {
            entity.ToTable("acessos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.Key).HasColumnName("chave").HasMaxLength(50);
            entity.Property(e => e.Name).HasColumnName("nome").HasMaxLength(100);
            entity.Property(e => e.GroupId).HasColumnName("grupo");
        });

        modelBuilder.Entity<LegacyProductRow>(entity =>
        {
            entity.ToTable("produtos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.CompanyId).HasColumnName("empresa");
            entity.Property(e => e.Code).HasColumnName("codigo").HasMaxLength(50);
            entity.Property(e => e.Name).HasColumnName("nome").HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnName("descricao").HasMaxLength(255);
            entity.Property(e => e.Stock).HasColumnName("estoque");
            entity.Property(e => e.SalePrice).HasColumnName("valor_venda");
            entity.Property(e => e.CostPrice).HasColumnName("valor_compra");
            entity.Property(e => e.ProfitMargin).HasColumnName("lucro");
            entity.Property(e => e.SupplierLegacyId).HasColumnName("fornecedor");
            entity.Property(e => e.CategoryLegacyId).HasColumnName("categoria");
            entity.Property(e => e.StockAlertLevel).HasColumnName("nivel_estoque");
            entity.Property(e => e.Active).HasColumnName("ativo").HasMaxLength(3);
            entity.Property(e => e.Photo).HasColumnName("foto").HasMaxLength(100);
        });

        modelBuilder.Entity<LegacyContractRow>(entity =>
        {
            entity.ToTable("contratos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.CompanyId).HasColumnName("empresa");
            entity.Property(e => e.Text).HasColumnName("texto");
            entity.Property(e => e.SignedDate).HasColumnName("data");
        });

        modelBuilder.Entity<LegacyConfigRow>(entity =>
        {
            entity.ToTable("config");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.CompanyId).HasColumnName("empresa");
            entity.Property(e => e.TrialDays).HasColumnName("dias_teste");
            entity.Property(e => e.BlockDays).HasColumnName("dias_bloqueio");
            entity.Property(e => e.BlockMessage).HasColumnName("msg_bloqueio");
            entity.Property(e => e.NomeSistema).HasColumnName("nome_sistema").HasMaxLength(100);
            entity.Property(e => e.Contatos).HasColumnName("contatos").HasMaxLength(200);
            entity.Property(e => e.CnpjSistema).HasColumnName("cnpj_sistema").HasMaxLength(20);
            entity.Property(e => e.Endereco).HasColumnName("endereco").HasMaxLength(200);
            entity.Property(e => e.TipoRel).HasColumnName("tipo_rel").HasMaxLength(10);
            entity.Property(e => e.TipoDesconto).HasColumnName("tipo_desconto").HasMaxLength(10);
            entity.Property(e => e.Comissao).HasColumnName("comissao");
            entity.Property(e => e.Token).HasColumnName("token").HasMaxLength(100);
            entity.Property(e => e.FotoRel).HasColumnName("foto_rel").HasMaxLength(200);
        });

        modelBuilder.Entity<LegacyPaymentMethodRow>(entity =>
        {
            entity.ToTable("forma_pgtos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.CompanyId).HasColumnName("empresa");
            entity.Property(e => e.Nome).HasColumnName("nome").HasMaxLength(50);
            entity.Property(e => e.Acrescimo).HasColumnName("acrescimo");
            entity.Property(e => e.Ativo).HasColumnName("ativo").HasMaxLength(3);
        });

        modelBuilder.Entity<LegacyCashRegisterRow>(entity =>
        {
            entity.ToTable("caixas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.CompanyId).HasColumnName("empresa");
            entity.Property(e => e.Nome).HasColumnName("nome").HasMaxLength(50);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(10);
            entity.Property(e => e.Operador).HasColumnName("operador");
        });

        modelBuilder.Entity<LegacyReceivableRow>(entity =>
        {
            entity.ToTable("receber");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.CompanyId).HasColumnName("empresa");
            entity.Property(e => e.Type).HasColumnName("tipo").HasMaxLength(20);
            entity.Property(e => e.PersonId).HasColumnName("pessoa");
            entity.Property(e => e.Amount).HasColumnName("valor");
            entity.Property(e => e.DueDate).HasColumnName("data_venc");
            entity.Property(e => e.Paid).HasColumnName("pago").HasMaxLength(3);
            entity.Property(e => e.CashSessionLegacyId).HasColumnName("caixa");
            entity.Property(e => e.PaymentMethodLegacyId).HasColumnName("forma_pgto");
            entity.Property(e => e.CustomerLegacyId).HasColumnName("cliente");
            entity.Property(e => e.Subtotal).HasColumnName("subtotal");
            entity.Property(e => e.Discount).HasColumnName("desconto");
            entity.Property(e => e.ChangeAmount).HasColumnName("troco");
            entity.Property(e => e.CommissionAmount).HasColumnName("comissao_venda");
            entity.Property(e => e.OperatorLegacyUserId).HasColumnName("operador");
            entity.Property(e => e.Cancelled).HasColumnName("cancelado").HasMaxLength(3);
            entity.Property(e => e.CompletedAt).HasColumnName("data_venda");
        });

        modelBuilder.Entity<LegacyCashSessionRow>(entity =>
        {
            entity.ToTable("caixa");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.CompanyId).HasColumnName("empresa");
            entity.Property(e => e.TerminalLegacyId).HasColumnName("terminal");
            entity.Property(e => e.OperatorLegacyUserId).HasColumnName("operador");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(10);
            entity.Property(e => e.OpeningFloat).HasColumnName("fundo_inicial");
            entity.Property(e => e.TotalSold).HasColumnName("total_vendido");
            entity.Property(e => e.CountedCash).HasColumnName("contado");
            entity.Property(e => e.Breakage).HasColumnName("quebra");
            entity.Property(e => e.OpenedAt).HasColumnName("data_abertura");
            entity.Property(e => e.ClosedAt).HasColumnName("data_fechamento");
        });

        modelBuilder.Entity<LegacyCartItemRow>(entity =>
        {
            entity.ToTable("itens_venda");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.CompanyId).HasColumnName("empresa");
            entity.Property(e => e.CashSessionLegacyId).HasColumnName("caixa");
            entity.Property(e => e.ProductLegacyId).HasColumnName("produto");
            entity.Property(e => e.Barcode).HasColumnName("codigo").HasMaxLength(50);
            entity.Property(e => e.Quantity).HasColumnName("quantidade");
            entity.Property(e => e.UnitPrice).HasColumnName("valor_unitario");
            entity.Property(e => e.SaleLegacyId).HasColumnName("venda");
            entity.Property(e => e.GradeOptionIds).HasColumnName("grade_opcoes");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
        });

        modelBuilder.Entity<LegacyWithdrawalRow>(entity =>
        {
            entity.ToTable("sangrias");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.CompanyId).HasColumnName("empresa");
            entity.Property(e => e.CashSessionLegacyId).HasColumnName("caixa");
            entity.Property(e => e.Amount).HasColumnName("valor");
            entity.Property(e => e.RegisteredAt).HasColumnName("data");
        });

        modelBuilder.Entity<LegacyCustomerRow>(entity =>
        {
            entity.ToTable("clientes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.CompanyId).HasColumnName("empresa");
            entity.Property(e => e.Nome).HasColumnName("nome").HasMaxLength(100);
            entity.Property(e => e.Cpf).HasColumnName("cpf").HasMaxLength(20);
            entity.Property(e => e.Telefone).HasColumnName("telefone").HasMaxLength(20);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(50);
            entity.Property(e => e.Endereco).HasColumnName("endereco").HasMaxLength(200);
            entity.Property(e => e.Ativo).HasColumnName("ativo").HasMaxLength(3);
        });

        modelBuilder.Entity<LegacySupplierRow>(entity =>
        {
            entity.ToTable("fornecedores");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.CompanyId).HasColumnName("empresa");
            entity.Property(e => e.Nome).HasColumnName("nome").HasMaxLength(100);
            entity.Property(e => e.Pessoa).HasColumnName("pessoa").HasMaxLength(10);
            entity.Property(e => e.Cpf).HasColumnName("cpf").HasMaxLength(20);
            entity.Property(e => e.Telefone).HasColumnName("telefone").HasMaxLength(20);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(50);
            entity.Property(e => e.Endereco).HasColumnName("endereco").HasMaxLength(200);
            entity.Property(e => e.Ativo).HasColumnName("ativo").HasMaxLength(3);
        });

        modelBuilder.Entity<LegacyAttachmentRow>(entity =>
        {
            entity.ToTable("arquivos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.CompanyId).HasColumnName("empresa");
            entity.Property(e => e.Tipo).HasColumnName("tipo").HasMaxLength(20);
            entity.Property(e => e.IdRef).HasColumnName("id_ref");
            entity.Property(e => e.Nome).HasColumnName("nome").HasMaxLength(100);
            entity.Property(e => e.Foto).HasColumnName("foto").HasMaxLength(500);
            entity.Property(e => e.DataValidade).HasColumnName("data_validade");
        });

        modelBuilder.Entity<LegacyCategoryRow>(entity =>
        {
            entity.ToTable("categorias");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.CompanyId).HasColumnName("empresa");
            entity.Property(e => e.Name).HasColumnName("nome").HasMaxLength(100);
            entity.Property(e => e.Active).HasColumnName("ativo").HasMaxLength(3);
        });

        modelBuilder.Entity<LegacyGradeDimensionRow>(entity =>
        {
            entity.ToTable("cat_grade");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.CompanyId).HasColumnName("empresa");
            entity.Property(e => e.ProductLegacyId).HasColumnName("produto");
            entity.Property(e => e.Name).HasColumnName("nome").HasMaxLength(50);
        });

        modelBuilder.Entity<LegacyGradeOptionRow>(entity =>
        {
            entity.ToTable("itens_grade");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.CompanyId).HasColumnName("empresa");
            entity.Property(e => e.DimensionLegacyId).HasColumnName("cat_grade");
            entity.Property(e => e.Label).HasColumnName("label").HasMaxLength(50);
            entity.Property(e => e.Stock).HasColumnName("estoque");
        });

        modelBuilder.Entity<LegacyStockEntryRow>(entity =>
        {
            entity.ToTable("entradas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.CompanyId).HasColumnName("empresa");
            entity.Property(e => e.ProductLegacyId).HasColumnName("produto");
            entity.Property(e => e.Quantity).HasColumnName("quantidade");
            entity.Property(e => e.Reason).HasColumnName("motivo").HasMaxLength(200);
            entity.Property(e => e.UserId).HasColumnName("usuario");
            entity.Property(e => e.CreatedAt).HasColumnName("data");
            entity.Property(e => e.MovementType).HasColumnName("tipo").HasMaxLength(20);
        });

        modelBuilder.Entity<LegacyStockExitRow>(entity =>
        {
            entity.ToTable("saidas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.CompanyId).HasColumnName("empresa");
            entity.Property(e => e.ProductLegacyId).HasColumnName("produto");
            entity.Property(e => e.Quantity).HasColumnName("quantidade");
            entity.Property(e => e.Reason).HasColumnName("motivo").HasMaxLength(200);
            entity.Property(e => e.UserId).HasColumnName("usuario");
            entity.Property(e => e.CreatedAt).HasColumnName("data");
            entity.Property(e => e.MovementType).HasColumnName("tipo").HasMaxLength(20);
        });

        modelBuilder.Entity<LegacyGradeMovementDetailRow>(entity =>
        {
            entity.ToTable("detalhes_grade");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            entity.Property(e => e.CompanyId).HasColumnName("empresa");
            entity.Property(e => e.MovementType).HasColumnName("tipo_movimento").HasMaxLength(20);
            entity.Property(e => e.MovementLegacyId).HasColumnName("id_movimento");
            entity.Property(e => e.OptionLegacyId).HasColumnName("id_item_grade");
            entity.Property(e => e.Quantity).HasColumnName("quantidade");
        });

        base.OnModelCreating(modelBuilder);
    }
}
