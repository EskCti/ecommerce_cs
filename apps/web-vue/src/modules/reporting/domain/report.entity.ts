export type ReportKind =
  | 'sales'
  | 'low-stock'
  | 'cash-sessions'
  | 'profit'
  | 'receipt'

export type ReportFilterInput = {
  readonly from?: string
  readonly to?: string
  readonly customerId?: string
  readonly sellerId?: string
  readonly status?: string
}

export type ReportCardData = {
  readonly id: ReportKind
  readonly title: string
  readonly description: string
  readonly requiresDateRange: boolean
}

export const REPORT_CARDS: readonly ReportCardData[] = [
  {
    id: 'sales',
    title: 'Vendas',
    description: 'Relatório de vendas por período, cliente e vendedor',
    requiresDateRange: true,
  },
  {
    id: 'low-stock',
    title: 'Estoque baixo',
    description: 'Produtos abaixo do nível mínimo configurado',
    requiresDateRange: false,
  },
  {
    id: 'cash-sessions',
    title: 'Caixas',
    description: 'Sessões de caixa por período de abertura',
    requiresDateRange: true,
  },
  {
    id: 'profit',
    title: 'Lucro',
    description: 'Demonstrativo simplificado receitas − custos − despesas',
    requiresDateRange: true,
  },
]
