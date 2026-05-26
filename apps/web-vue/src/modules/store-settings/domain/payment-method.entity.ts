import { err, ok, type Result } from '@/shared/result'

export type PaymentMethodData = {
  readonly id: string
  readonly name: string
  readonly surchargePercent: number
  readonly isActive: boolean
  readonly createdAt: string
  readonly updatedAt: string
}

export class PaymentMethodEntity implements PaymentMethodData {
  readonly id: string
  readonly name: string
  readonly surchargePercent: number
  readonly isActive: boolean
  readonly createdAt: string
  readonly updatedAt: string

  private constructor(data: PaymentMethodData) {
    this.id = data.id
    this.name = data.name
    this.surchargePercent = data.surchargePercent
    this.isActive = data.isActive
    this.createdAt = data.createdAt
    this.updatedAt = data.updatedAt
  }

  static create(data: PaymentMethodData): Result<PaymentMethodEntity> {
    if (!data.name.trim()) return err('Nome da forma de pagamento é obrigatório')
    if (data.surchargePercent < 0 || data.surchargePercent > 100) {
      return err('Acréscimo deve estar entre 0 e 100')
    }
    return ok(new PaymentMethodEntity(data))
  }

  static fromApi(data: PaymentMethodData): Result<PaymentMethodEntity> {
    return PaymentMethodEntity.create(data)
  }
}
