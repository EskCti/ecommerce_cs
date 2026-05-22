import { err, type Result } from '@/shared/result'
import type { AuthSessionEntity } from '../domain/auth-session.entity'
import type { IAuthRepository, LoginInput } from './auth.repository'

export class LoginUseCase {
  constructor(private readonly repository: IAuthRepository) {}

  async execute(input: LoginInput): Promise<Result<AuthSessionEntity>> {
    const login = input.login.trim()
    if (!login) return err('Informe e-mail ou CPF')
    if (!input.password) return err('Informe a senha')
    return this.repository.login({ login, password: input.password })
  }
}
