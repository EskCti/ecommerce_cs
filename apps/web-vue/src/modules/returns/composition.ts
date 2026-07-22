import {
  DeleteExchangeUseCase,
  ListExchangesUseCase,
  RegisterExchangeUseCase,
} from './application/returns.usecase'
import { ReturnsHttpRepository } from './infrastructure/returns-http.repository'

export function createReturnsModule(getToken: () => string | null) {
  const repository = new ReturnsHttpRepository(getToken)

  return {
    registerExchangeUseCase: new RegisterExchangeUseCase(repository),
    listExchangesUseCase: new ListExchangesUseCase(repository),
    deleteExchangeUseCase: new DeleteExchangeUseCase(repository),
  }
}
