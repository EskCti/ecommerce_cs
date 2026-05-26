import { err, ok, type Result } from '@/shared/result'

export type CashRegisterTerminalData = {
  readonly id: string
  readonly name: string
  readonly status: string
  readonly assignedOperatorId?: string | null
  readonly assignedOperatorName?: string | null
  readonly createdAt: string
  readonly updatedAt: string
}

export class CashRegisterTerminalEntity implements CashRegisterTerminalData {
  readonly id: string
  readonly name: string
  readonly status: string
  readonly assignedOperatorId?: string | null
  readonly assignedOperatorName?: string | null
  readonly createdAt: string
  readonly updatedAt: string

  private constructor(data: CashRegisterTerminalData) {
    this.id = data.id
    this.name = data.name
    this.status = data.status
    this.assignedOperatorId = data.assignedOperatorId
    this.assignedOperatorName = data.assignedOperatorName
    this.createdAt = data.createdAt
    this.updatedAt = data.updatedAt
  }

  static create(data: CashRegisterTerminalData): Result<CashRegisterTerminalEntity> {
    if (!data.name.trim()) return err('Nome do caixa é obrigatório')
    return ok(new CashRegisterTerminalEntity(data))
  }

  static fromApi(data: CashRegisterTerminalData): Result<CashRegisterTerminalEntity> {
    return CashRegisterTerminalEntity.create(data)
  }
}
