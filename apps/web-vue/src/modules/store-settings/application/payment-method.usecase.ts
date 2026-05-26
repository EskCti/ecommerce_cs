import { ok, type Result } from '@/shared/result'
import type { IStoreSettingsRepository, PaymentMethodInput, CashRegisterInput } from './store-settings.repository'
import type { StoreConfigEntity } from '../domain/store-config.entity'
import type { PaymentMethodEntity } from '../domain/payment-method.entity'
import type { CashRegisterTerminalEntity } from '../domain/cash-register-terminal.entity'

export class GetStoreConfigUseCase {
  constructor(private readonly repository: IStoreSettingsRepository) {}
  execute(): Promise<Result<StoreConfigEntity>> {
    return this.repository.getStoreConfig()
  }
}

export class UpdateStoreConfigUseCase {
  constructor(private readonly repository: IStoreSettingsRepository) {}
  execute(config: StoreConfigEntity): Promise<Result<StoreConfigEntity>> {
    return this.repository.updateStoreConfig(config)
  }
}

export class ListPaymentMethodsUseCase {
  constructor(private readonly repository: IStoreSettingsRepository) {}
  execute(): Promise<Result<PaymentMethodEntity[]>> {
    return this.repository.listPaymentMethods()
  }
}

export class CreatePaymentMethodUseCase {
  constructor(private readonly repository: IStoreSettingsRepository) {}
  execute(input: PaymentMethodInput): Promise<Result<PaymentMethodEntity>> {
    return this.repository.createPaymentMethod(input)
  }
}

export class UpdatePaymentMethodUseCase {
  constructor(private readonly repository: IStoreSettingsRepository) {}
  execute(id: string, input: Partial<PaymentMethodInput>): Promise<Result<PaymentMethodEntity>> {
    return this.repository.updatePaymentMethod(id, input)
  }
}

export class DeletePaymentMethodUseCase {
  constructor(private readonly repository: IStoreSettingsRepository) {}
  async execute(id: string): Promise<Result<void>> {
    const result = await this.repository.deletePaymentMethod(id)
    if (!result.ok) return result
    return ok(undefined)
  }
}

export class ListCashRegistersUseCase {
  constructor(private readonly repository: IStoreSettingsRepository) {}
  execute(): Promise<Result<CashRegisterTerminalEntity[]>> {
    return this.repository.listCashRegisters()
  }
}

export class CreateCashRegisterUseCase {
  constructor(private readonly repository: IStoreSettingsRepository) {}
  execute(input: CashRegisterInput): Promise<Result<CashRegisterTerminalEntity>> {
    return this.repository.createCashRegister(input)
  }
}

export class UpdateCashRegisterUseCase {
  constructor(private readonly repository: IStoreSettingsRepository) {}
  execute(id: string, input: Partial<CashRegisterInput>): Promise<Result<CashRegisterTerminalEntity>> {
    return this.repository.updateCashRegister(id, input)
  }
}

export class DeleteCashRegisterUseCase {
  constructor(private readonly repository: IStoreSettingsRepository) {}
  async execute(id: string): Promise<Result<void>> {
    const result = await this.repository.deleteCashRegister(id)
    if (!result.ok) return result
    return ok(undefined)
  }
}
