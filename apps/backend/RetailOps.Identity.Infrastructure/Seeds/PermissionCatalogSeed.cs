using RetailOps.Identity.Core.Application.Dtos;

namespace RetailOps.Identity.Infrastructure.Seeds;

public static class PermissionCatalogSeed
{
    public static IReadOnlyList<PermissionCatalogItemDto> Items { get; } =
    [
        new("home", "Início", "Geral"),
        new("empresas", "Empresas", "SAS"),
        new("usuarios", "Usuários", "Pessoas"),
        new("cargos", "Cargos", "Pessoas"),
        new("clientes", "Clientes", "Cadastros"),
        new("fornecedores", "Fornecedores", "Cadastros"),
        new("produtos", "Produtos", "Produtos"),
        new("categorias", "Categorias", "Produtos"),
        new("marcas", "Marcas", "Produtos"),
        new("estoque", "Estoque", "Produtos"),
        new("abertura", "Abertura de caixa", "Financeiro"),
        new("fechamento", "Fechamento de caixa", "Financeiro"),
        new("vendas", "Vendas", "Financeiro"),
        new("receber", "Contas a receber", "Financeiro"),
        new("pagar", "Contas a pagar", "Financeiro"),
        new("caixa", "Caixa", "Financeiro"),
        new("rel_vendas", "Relatório de vendas", "Relatórios"),
        new("rel_estoque", "Relatório de estoque", "Relatórios"),
        new("rel_financeiro", "Relatório financeiro", "Relatórios"),
        new("config", "Configurações", "Sistema"),
        new("permissoes", "Permissões", "Sistema"),
        new("dashboard", "Dashboard", "Geral"),
        new("pdv", "PDV", "Vendas"),
        new("orcamentos", "Orçamentos", "Vendas"),
        new("devolucoes", "Devoluções", "Vendas"),
        new("comissoes", "Comissões", "Pessoas"),
        new("promocoes", "Promoções", "Produtos"),
        new("nfce", "NFC-e", "Fiscal"),
        new("nfe", "NF-e", "Fiscal"),
        new("backup", "Backup", "Sistema"),
        new("auditoria", "Auditoria", "Sistema"),
        new("integracao", "Integrações", "Sistema"),
        new("rel_clientes", "Relatório de clientes", "Relatórios"),
        new("rel_produtos", "Relatório de produtos", "Relatórios"),
        new("rel_caixa", "Relatório de caixa", "Relatórios"),
    ];
}
