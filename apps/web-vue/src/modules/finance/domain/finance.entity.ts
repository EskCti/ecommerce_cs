import { err, ok, type Result } from '@/shared/result'

export type PaymentStatus = 'Open' | 'Settled'
export type AccountType = 'Expense' | 'Purchase' | 'CommissionPayment'

export type ReceivableData = {
  readonly id: string
  readonly description: string
  readonly amount: number
  readonly currency: string
  readonly dueDate: string
  readonly status: PaymentStatus
  readonly settledAt?: string
  readonly saleId?: string
  readonly personLegacyId?: number
  readonly createdAt: string
  readonly updatedAt: string
}

export class ReceivableEntity implements ReceivableData {
  readonly id: string
  readonly description: string
  readonly amount: number
  readonly currency: string
  readonly dueDate: string
  readonly status: PaymentStatus
  readonly settledAt?: string
  readonly saleId?: string
  readonly personLegacyId?: number
  readonly createdAt: string
  readonly updatedAt: string

  private constructor(data: ReceivableData) {
    Object.assign(this, data)
  }

  static fromApi(data: ReceivableData): Result<ReceivableEntity> {
    const id = String(data.id ?? '').trim()
    const description = String(data.description ?? '').trim() || 'Sem descrição'
    const amount = Number(data.amount)
    if (!id) return err('Id é obrigatório')
    if (!Number.isFinite(amount) || amount <= 0) return err('Valor deve ser positivo')
    return ok(new ReceivableEntity({ ...data, id, description, amount }))
  }
}

export type PayableData = {
  readonly id: string
  readonly type: AccountType
  readonly description: string
  readonly amount: number
  readonly currency: string
  readonly dueDate: string
  readonly status: PaymentStatus
  readonly settledAt?: string
  readonly recurrenceDays?: number
  readonly personLegacyId?: number
  readonly productId?: string
  readonly createdAt: string
  readonly updatedAt: string
}

export class PayableEntity implements PayableData {
  readonly id!: string
  readonly type!: AccountType
  readonly description!: string
  readonly amount!: number
  readonly currency!: string
  readonly dueDate!: string
  readonly status!: PaymentStatus
  readonly settledAt?: string
  readonly recurrenceDays?: number
  readonly personLegacyId?: number
  readonly productId?: string
  readonly createdAt!: string
  readonly updatedAt!: string

  private constructor(data: PayableData) {
    Object.assign(this, data)
  }

  static fromApi(data: PayableData): Result<PayableEntity> {
    const id = String(data.id ?? '').trim()
    const description = String(data.description ?? '').trim() || 'Sem descrição'
    const amount = Number(data.amount)
    if (!id) return err('Id é obrigatório')
    if (!Number.isFinite(amount) || amount <= 0) return err('Valor deve ser positivo')
    return ok(new PayableEntity({ ...data, id, description, amount }))
  }
}

export type CommissionData = {
  readonly id: string
  readonly saleId: string
  readonly sellerLegacyId: number
  readonly amount: number
  readonly currency: string
  readonly isPaid: boolean
  readonly paidAt?: string
  readonly createdAt: string
  readonly updatedAt: string
}

export class CommissionEntity implements CommissionData {
  readonly id!: string
  readonly saleId!: string
  readonly sellerLegacyId!: number
  readonly amount!: number
  readonly currency!: string
  readonly isPaid!: boolean
  readonly paidAt?: string
  readonly createdAt!: string
  readonly updatedAt!: string

  private constructor(data: CommissionData) {
    Object.assign(this, data)
  }

  static fromApi(data: CommissionData): Result<CommissionEntity> {
    if (!data.id?.trim()) return err('Id é obrigatório')
    return ok(new CommissionEntity(data))
  }
}

export type CashFlowData = {
  readonly totalInflow: number
  readonly totalOutflow: number
  readonly balance: number
}
