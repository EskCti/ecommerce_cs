import {
  AddCartItemUseCase,
  CancelSaleUseCase,
  CloseCashSessionUseCase,
  ConfirmGradeForItemUseCase,
  FinalizeSaleUseCase,
  GetCurrentCashSessionUseCase,
  ListSalesUseCase,
  OpenCashSessionUseCase,
  RegisterCashWithdrawalUseCase,
  RemoveCartLineUseCase,
} from './application/sales.usecase'
import { SalesHttpRepository } from './infrastructure/sales-http.repository'

export function createSalesModule(getToken: () => string | null) {
  const repository = new SalesHttpRepository(getToken)

  return {
    openCashSessionUseCase: new OpenCashSessionUseCase(repository),
    getCurrentCashSessionUseCase: new GetCurrentCashSessionUseCase(repository),
    addCartItemUseCase: new AddCartItemUseCase(repository),
    confirmGradeForItemUseCase: new ConfirmGradeForItemUseCase(repository),
    removeCartLineUseCase: new RemoveCartLineUseCase(repository),
    registerCashWithdrawalUseCase: new RegisterCashWithdrawalUseCase(repository),
    closeCashSessionUseCase: new CloseCashSessionUseCase(repository),
    finalizeSaleUseCase: new FinalizeSaleUseCase(repository),
    listSalesUseCase: new ListSalesUseCase(repository),
    cancelSaleUseCase: new CancelSaleUseCase(repository),
  }
}
