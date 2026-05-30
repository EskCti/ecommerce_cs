import {
  CreatePayableUseCase,
  CreateReceivableUseCase,
  GetCashFlowUseCase,
  ListCommissionsUseCase,
  ListPayablesUseCase,
  ListPurchasesUseCase,
  ListReceivablesUseCase,
  PayCommissionUseCase,
  PayCommissionsBatchUseCase,
  SettlePayableUseCase,
  SettleReceivableUseCase,
} from './application/finance.usecase'
import { FinanceHttpRepository } from './infrastructure/finance-http.repository'

export function createFinanceModule(getToken: () => string | null) {
  const repository = new FinanceHttpRepository(getToken)

  return {
    listReceivablesUseCase: new ListReceivablesUseCase(repository),
    createReceivableUseCase: new CreateReceivableUseCase(repository),
    settleReceivableUseCase: new SettleReceivableUseCase(repository),
    listPayablesUseCase: new ListPayablesUseCase(repository),
    createPayableUseCase: new CreatePayableUseCase(repository),
    settlePayableUseCase: new SettlePayableUseCase(repository),
    listPurchasesUseCase: new ListPurchasesUseCase(repository),
    listCommissionsUseCase: new ListCommissionsUseCase(repository),
    payCommissionUseCase: new PayCommissionUseCase(repository),
    payCommissionsBatchUseCase: new PayCommissionsBatchUseCase(repository),
    getCashFlowUseCase: new GetCashFlowUseCase(repository),
    repository,
  }
}
