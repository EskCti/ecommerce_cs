import { err, ok, type Result } from '@/shared/result'
import { AuthUserEntity } from './auth-user.entity'

export type AuthSessionData = {
  readonly accessToken: string
  readonly user: AuthUserEntity
}

export class AuthSessionEntity {
  readonly accessToken: string
  readonly user: AuthUserEntity

  private constructor(data: AuthSessionData) {
    this.accessToken = data.accessToken
    this.user = data.user
  }

  static create(accessToken: string, user: AuthUserEntity): Result<AuthSessionEntity> {
    if (!accessToken.trim()) return err('Token inválido')
    return ok(new AuthSessionEntity({ accessToken, user }))
  }
}
