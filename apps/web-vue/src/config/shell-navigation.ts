export type SidebarMenuItem = {
  id: string
  label: string
  to: string
  match?: 'exact' | 'prefix'
}

export type SidebarMenuSection = {
  id: string
  label?: string
  items: SidebarMenuItem[]
}

export const DEFAULT_SHELL_MAIN_ITEM: SidebarMenuItem = {
  id: 'dashboard',
  label: 'Dashboard',
  to: '/dashboard',
  match: 'exact',
}

export const DEFAULT_SHELL_SECTIONS: SidebarMenuSection[] = [
  {
    id: 'scope',
    label: 'Escopo',
    items: [
      { id: 'platform', label: 'Plataforma (SAS)', to: '/platform', match: 'prefix' },
      { id: 'tenant', label: 'Loja (Tenant)', to: '/tenant', match: 'prefix' },
    ],
  },
  {
    id: 'modules',
    label: 'Módulos',
    items: [
      {
        id: 'tenant-settings-store',
        label: 'Configuração da loja',
        to: '/tenant/settings/store',
        match: 'prefix',
      },
      {
        id: 'tenant-settings-payment-methods',
        label: 'Formas de pagamento',
        to: '/tenant/settings/payment-methods',
        match: 'prefix',
      },
      {
        id: 'tenant-settings-cash-registers',
        label: 'Caixas físicos',
        to: '/tenant/settings/cash-registers',
        match: 'prefix',
      },
      {
        id: 'tenant-crm-customers',
        label: 'Clientes',
        to: '/tenant/crm/customers',
        match: 'prefix',
      },
      {
        id: 'tenant-crm-suppliers',
        label: 'Fornecedores',
        to: '/tenant/crm/suppliers',
        match: 'prefix',
      },
      {
        id: 'tenant-catalog-products',
        label: 'Produtos',
        to: '/tenant/catalog/products',
        match: 'prefix',
      },
      {
        id: 'tenant-catalog-categories',
        label: 'Categorias',
        to: '/tenant/catalog/categories',
        match: 'prefix',
      },
      {
        id: 'tenant-catalog-low-stock',
        label: 'Estoque baixo',
        to: '/tenant/catalog/inventory/low-stock',
        match: 'prefix',
      },
      {
        id: 'tenant-catalog-stock-movements',
        label: 'Movimentação estoque',
        to: '/tenant/catalog/stock/movements',
        match: 'prefix',
      },
      {
        id: 'tenant-pdv',
        label: 'PDV',
        to: '/tenant/pdv',
        match: 'prefix',
      },
      {
        id: 'tenant-sales',
        label: 'Vendas',
        to: '/tenant/sales',
        match: 'prefix',
      },
      {
        id: 'tenant-finance-receivables',
        label: 'Contas a receber',
        to: '/tenant/finance/receivables',
        match: 'prefix',
      },
      {
        id: 'tenant-finance-payables',
        label: 'Despesas',
        to: '/tenant/finance/payables',
        match: 'prefix',
      },
      {
        id: 'tenant-finance-purchases',
        label: 'Compras',
        to: '/tenant/finance/purchases',
        match: 'prefix',
      },
      {
        id: 'tenant-finance-commissions',
        label: 'Comissões / Fluxo',
        to: '/tenant/finance/commissions',
        match: 'prefix',
      },
    ],
  },
]
