import { GetStoreConfigUseCase, UpdateStoreConfigUseCase } from './application/payment-method.usecase'
import {
  CreatePaymentMethodUseCase,
  DeletePaymentMethodUseCase,
  ListPaymentMethodsUseCase,
  UpdatePaymentMethodUseCase,
  CreateCashRegisterUseCase,
  DeleteCashRegisterUseCase,
  ListCashRegistersUseCase,
  UpdateCashRegisterUseCase,
} from './application/payment-method.usecase'
import { StoreSettingsHttpRepository } from './infrastructure/store-settings-http.repository'

export function createStoreSettingsModule(getToken: () => string | null) {
  const repository = new StoreSettingsHttpRepository(getToken)
  return {
    getStoreConfigUseCase: new GetStoreConfigUseCase(repository),
    updateStoreConfigUseCase: new UpdateStoreConfigUseCase(repository),
    listPaymentMethodsUseCase: new ListPaymentMethodsUseCase(repository),
    createPaymentMethodUseCase: new CreatePaymentMethodUseCase(repository),
    updatePaymentMethodUseCase: new UpdatePaymentMethodUseCase(repository),
    deletePaymentMethodUseCase: new DeletePaymentMethodUseCase(repository),
    listCashRegistersUseCase: new ListCashRegistersUseCase(repository),
    createCashRegisterUseCase: new CreateCashRegisterUseCase(repository),
    updateCashRegisterUseCase: new UpdateCashRegisterUseCase(repository),
    deleteCashRegisterUseCase: new DeleteCashRegisterUseCase(repository),
  }
}
