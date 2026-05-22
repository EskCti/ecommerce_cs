import type { Result } from '@/shared/result'
import type { AuthUserEntity } from '../domain/auth-user.entity'
import type { IAuthRepository } from './auth.repository'

export class RestoreSessionUseCase {
  constructor(private readonly repository: IAuthRepository) {}

  async execute(): Promise<Result<AuthUserEntity>> {
    return this.repository.getCurrentUser()
  }
}
