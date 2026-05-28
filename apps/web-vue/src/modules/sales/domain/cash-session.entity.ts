import { err, ok, type Result } from '@/shared/result'

export type CartLineData = {
  readonly id: string
  readonly productId: string
  readonly barcode: string
  readonly quantity: number
  readonly unitPrice: number
  readonly lineTotal: number
  readonly status: string
  readonly requiresGrade: boolean
  readonly gradeOptionIds: readonly string[]
}

export type CashSessionData = {
  readonly id: string
  readonly tenantId: number
  readonly terminalId: string
  readonly operatorUserId: string
  readonly status: string
  readonly openingFloat: number
  readonly totalSold: number
  readonly totalWithdrawals: number
  readonly countedCash?: number | null
  readonly breakage?: number | null
  readonly openedAt: string
  readonly closedAt?: string | null
  readonly lines: readonly CartLineData[]
}

export class CartLineEntity implements CartLineData {
  readonly id: string
  readonly productId: string
  readonly barcode: string
  readonly quantity: number
  readonly unitPrice: number
  readonly lineTotal: number
  readonly status: string
  readonly requiresGrade: boolean
  readonly gradeOptionIds: readonly string[]

  private constructor(data: CartLineData) {
    this.id = data.id
    this.productId = data.productId
    this.barcode = data.barcode
    this.quantity = data.quantity
    this.unitPrice = data.unitPrice
    this.lineTotal = data.lineTotal
    this.status = data.status
    this.requiresGrade = data.requiresGrade
    this.gradeOptionIds = data.gradeOptionIds
  }

  static fromApi(data: CartLineData): Result<CartLineEntity> {
    if (!data.id) return err('Id da linha é obrigatório')
    return ok(new CartLineEntity(data))
  }

  get isPendingGrade(): boolean {
    return this.status === 'PendingGrade'
  }
}

export class CashSessionEntity implements CashSessionData {
  readonly id: string
  readonly tenantId: number
  readonly terminalId: string
  readonly operatorUserId: string
  readonly status: string
  readonly openingFloat: number
  readonly totalSold: number
  readonly totalWithdrawals: number
  readonly countedCash?: number | null
  readonly breakage?: number | null
  readonly openedAt: string
  readonly closedAt?: string | null
  readonly lines: readonly CartLineEntity[]

  private constructor(data: CashSessionData, lines: CartLineEntity[]) {
    this.id = data.id
    this.tenantId = data.tenantId
    this.terminalId = data.terminalId
    this.operatorUserId = data.operatorUserId
    this.status = data.status
    this.openingFloat = data.openingFloat
    this.totalSold = data.totalSold
    this.totalWithdrawals = data.totalWithdrawals
    this.countedCash = data.countedCash
    this.breakage = data.breakage
    this.openedAt = data.openedAt
    this.closedAt = data.closedAt
    this.lines = lines
  }

  static fromApi(data: CashSessionData): Result<CashSessionEntity> {
    if (!data.id) return err('Id da sessão é obrigatório')
    const lines: CartLineEntity[] = []
    for (const line of data.lines ?? []) {
      const mapped = CartLineEntity.fromApi(line)
      if (!mapped.ok) return mapped
      lines.push(mapped.data)
    }
    return ok(new CashSessionEntity(data, lines))
  }

  get isOpen(): boolean {
    return this.status === 'Open'
  }

  get cartSubtotal(): number {
    return this.lines.reduce((sum, line) => sum + line.lineTotal, 0)
  }

  get expectedCashInDrawer(): number {
    return this.openingFloat + this.totalSold - this.totalWithdrawals
  }

  get pendingGradeLine(): CartLineEntity | undefined {
    return this.lines.find((line) => line.isPendingGrade)
  }
}
