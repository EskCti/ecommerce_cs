import { err, type Result } from '@/shared/result'
import type { IAuthRepository, LoginInput } from '../application/auth.repository'
import { AuthSessionEntity } from '../domain/auth-session.entity'
import { AuthUserEntity } from '../domain/auth-user.entity'
import type { LoginResponseDto, MeResponseDto } from './auth.dto'

export type TokenProvider = () => string | null

export class AuthHttpRepository implements IAuthRepository {
  constructor(
    private readonly getToken: TokenProvider,
    private readonly setToken: (token: string | null) => void,
  ) {}

  async login(input: LoginInput): Promise<Result<AuthSessionEntity>> {
    try {
      const res = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ login: input.login, password: input.password }),
      })
      if (!res.ok) {
        const body = (await res.json().catch(() => ({}))) as { error?: string }
        return err(body.error ?? 'Falha no login')
      }
      const data = (await res.json()) as LoginResponseDto
      this.setToken(data.accessToken)
      const me = await this.getCurrentUser()
      if (!me.ok) return me
      return AuthSessionEntity.create(data.accessToken, me.data)
    } catch {
      return err('Erro de rede')
    }
  }

  async getCurrentUser(): Promise<Result<AuthUserEntity>> {
    const token = this.getToken()
    if (!token) return err('Sessão expirada')

    try {
      const res = await fetch('/api/auth/me', {
        headers: { Authorization: `Bearer ${token}` },
      })
      if (!res.ok) return err(res.status === 401 ? 'Sessão expirada' : 'Falha ao carregar perfil')
      const data = (await res.json()) as MeResponseDto
      return AuthUserEntity.fromApi({
        id: data.id,
        legacyUserId: data.legacyUserId,
        tenantId: data.tenantId,
        name: data.name,
        email: data.email,
        userLevel: data.userLevel,
        permissionKeys: data.permissionKeys,
      })
    } catch {
      return err('Erro de rede')
    }
  }
}
