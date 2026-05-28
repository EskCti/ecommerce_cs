import { err, ok, type Result } from '@/shared/result'
import { CartLineEntity, type CartLineData } from './cash-session.entity'

export type SaleData = {
  readonly id: string
  readonly tenantId: number
  readonly cashSessionId: string
  readonly operatorUserId: string
  readonly paymentTerms: string
  readonly customerId?: string | null
  readonly paymentMethodId: string
  readonly subtotal: number
  readonly discount: number
  readonly total: number
  readonly change: number
  readonly commissionAmount: number
  readonly isCancelled: boolean
  readonly completedAt: string
  readonly cancelledAt?: string | null
  readonly lines: readonly CartLineData[]
}

export type SaleListItemData = {
  readonly id: string
  readonly total: number
  readonly paymentTerms: string
  readonly isCancelled: boolean
  readonly completedAt: string
}

export type SaleListPageData = {
  readonly items: readonly SaleListItemData[]
  readonly page: number
  readonly pageSize: number
  readonly totalCount: number
}

export class SaleEntity implements SaleData {
  readonly id: string
  readonly tenantId: number
  readonly cashSessionId: string
  readonly operatorUserId: string
  readonly paymentTerms: string
  readonly customerId?: string | null
  readonly paymentMethodId: string
  readonly subtotal: number
  readonly discount: number
  readonly total: number
  readonly change: number
  readonly commissionAmount: number
  readonly isCancelled: boolean
  readonly completedAt: string
  readonly cancelledAt?: string | null
  readonly lines: readonly CartLineEntity[]

  private constructor(data: SaleData, lines: CartLineEntity[]) {
    this.id = data.id
    this.tenantId = data.tenantId
    this.cashSessionId = data.cashSessionId
    this.operatorUserId = data.operatorUserId
    this.paymentTerms = data.paymentTerms
    this.customerId = data.customerId
    this.paymentMethodId = data.paymentMethodId
    this.subtotal = data.subtotal
    this.discount = data.discount
    this.total = data.total
    this.change = data.change
    this.commissionAmount = data.commissionAmount
    this.isCancelled = data.isCancelled
    this.completedAt = data.completedAt
    this.cancelledAt = data.cancelledAt
    this.lines = lines
  }

  static fromApi(data: SaleData): Result<SaleEntity> {
    if (!data.id) return err('Id da venda é obrigatório')
    const lines: CartLineEntity[] = []
    for (const line of data.lines ?? []) {
      const mapped = CartLineEntity.fromApi(line)
      if (!mapped.ok) return mapped
      lines.push(mapped.data)
    }
    return ok(new SaleEntity(data, lines))
  }
}

export class SaleListItemEntity implements SaleListItemData {
  readonly id: string
  readonly total: number
  readonly paymentTerms: string
  readonly isCancelled: boolean
  readonly completedAt: string

  private constructor(data: SaleListItemData) {
    this.id = data.id
    this.total = data.total
    this.paymentTerms = data.paymentTerms
    this.isCancelled = data.isCancelled
    this.completedAt = data.completedAt
  }

  static fromApi(data: SaleListItemData): Result<SaleListItemEntity> {
    if (!data.id) return err('Id da venda é obrigatório')
    return ok(new SaleListItemEntity(data))
  }
}

export class SaleListPageEntity implements SaleListPageData {
  readonly items: readonly SaleListItemEntity[]
  readonly page: number
  readonly pageSize: number
  readonly totalCount: number

  private constructor(data: SaleListPageData, items: SaleListItemEntity[]) {
    this.items = items
    this.page = data.page
    this.pageSize = data.pageSize
    this.totalCount = data.totalCount
  }

  static fromApi(data: SaleListPageData): Result<SaleListPageEntity> {
    const items: SaleListItemEntity[] = []
    for (const row of data.items ?? []) {
      const mapped = SaleListItemEntity.fromApi(row)
      if (!mapped.ok) return mapped
      items.push(mapped.data)
    }
    return ok(
      new SaleListPageEntity(
        {
          items: data.items,
          page: data.page ?? 1,
          pageSize: data.pageSize ?? 20,
          totalCount: data.totalCount ?? items.length,
        },
        items,
      ),
    )
  }
}
