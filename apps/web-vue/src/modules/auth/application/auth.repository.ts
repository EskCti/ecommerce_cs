import type { AuthSessionEntity } from '../domain/auth-session.entity'
import type { AuthUserEntity } from '../domain/auth-user.entity'
import type { Result } from '@/shared/result'

export type LoginInput = {
  readonly login: string
  readonly password: string
}

export interface IAuthRepository {
  login(input: LoginInput): Promise<Result<AuthSessionEntity>>
  getCurrentUser(): Promise<Result<AuthUserEntity>>
}
