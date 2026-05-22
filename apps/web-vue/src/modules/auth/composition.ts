import { AuthHttpRepository } from './infrastructure/auth-http.repository'
import { LoginUseCase } from './application/login.usecase'
import { RestoreSessionUseCase } from './application/restore-session.usecase'

export function createAuthModule(getToken: () => string | null, setToken: (t: string | null) => void) {
  const repository = new AuthHttpRepository(getToken, setToken)
  return {
    repository,
    loginUseCase: new LoginUseCase(repository),
    restoreSessionUseCase: new RestoreSessionUseCase(repository),
  }
}
