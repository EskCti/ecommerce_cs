import { describe, expect, it, vi } from 'vitest'
import { LoginUseCase } from './login.usecase'
import { err } from '@/shared/result'
import { AuthUserEntity } from '../domain/auth-user.entity'
import { AuthSessionEntity } from '../domain/auth-session.entity'
import type { IAuthRepository } from './auth.repository'

describe('LoginUseCase', () => {
  it('rejects empty login', async () => {
    const repo: IAuthRepository = {
      login: vi.fn(),
      getCurrentUser: vi.fn(),
    }
    const result = await new LoginUseCase(repo).execute({ login: '  ', password: 'x' })
    expect(result.ok).toBe(false)
    if (!result.ok) expect(result.error).toContain('e-mail')
  })

  it('delegates to repository', async () => {
    const user = AuthUserEntity.fromApi({
      id: 'u1',
      legacyUserId: 1,
      tenantId: 1,
      name: 'Test',
      userLevel: 'Administrador',
      permissionKeys: [],
    })
    if (!user.ok) throw new Error('fixture')
    const session = AuthSessionEntity.create('token', user.data)
    if (!session.ok) throw new Error('fixture')

    const login = vi.fn().mockResolvedValue(session)
    const repo: IAuthRepository = { login, getCurrentUser: vi.fn() }
    const result = await new LoginUseCase(repo).execute({ login: 'a@b.com', password: 'secret' })
    expect(login).toHaveBeenCalled()
    expect(result.ok).toBe(true)
  })

  it('propagates repository error', async () => {
    const repo: IAuthRepository = {
      login: vi.fn().mockResolvedValue(err('Credenciais inválidas')),
      getCurrentUser: vi.fn(),
    }
    const result = await new LoginUseCase(repo).execute({ login: 'a@b.com', password: 'bad' })
    expect(result.ok).toBe(false)
    if (!result.ok) expect(result.error).toBe('Credenciais inválidas')
  })
})
