import type { StoreConfigEntity } from '../domain/store-config.entity'
import type { PaymentMethodEntity } from '../domain/payment-method.entity'
import type { CashRegisterTerminalEntity } from '../domain/cash-register-terminal.entity'
import type { Result } from '@/shared/result'

export type PaymentMethodInput = {
  name: string
  surchargePercent: number
  isActive: boolean
}

export type CashRegisterInput = {
  name: string
  status?: string
  assignedOperatorId?: string | null
}

export interface IStoreSettingsRepository {
  getStoreConfig(): Promise<Result<StoreConfigEntity>>
  updateStoreConfig(config: StoreConfigEntity): Promise<Result<StoreConfigEntity>>
  listPaymentMethods(): Promise<Result<PaymentMethodEntity[]>>
  createPaymentMethod(input: PaymentMethodInput): Promise<Result<PaymentMethodEntity>>
  updatePaymentMethod(id: string, input: Partial<PaymentMethodInput>): Promise<Result<PaymentMethodEntity>>
  deletePaymentMethod(id: string): Promise<Result<void>>
  listCashRegisters(): Promise<Result<CashRegisterTerminalEntity[]>>
  createCashRegister(input: CashRegisterInput): Promise<Result<CashRegisterTerminalEntity>>
  updateCashRegister(id: string, input: Partial<CashRegisterInput>): Promise<Result<CashRegisterTerminalEntity>>
  deleteCashRegister(id: string): Promise<Result<void>>
}
