import { describe, expect, it } from 'vitest'
import { AuthUserEntity } from './auth-user.entity'

describe('AuthUserEntity', () => {
  const base = {
    id: 'u1',
    legacyUserId: 1,
    tenantId: 1,
    name: 'Test',
    userLevel: 'Operador',
    permissionKeys: ['vendas'],
  }

  it('allows privileged users any permission', () => {
    const admin = AuthUserEntity.fromApi({ ...base, userLevel: 'Administrador', permissionKeys: [] })
    expect(admin.ok && admin.data.hasPermission('usuarios')).toBe(true)
  })

  it('blocks operator without usuarios permission (router guard scenario)', () => {
    const user = AuthUserEntity.fromApi(base)
    expect(user.ok && user.data.hasPermission('usuarios')).toBe(false)
  })

  it('allows operator with usuarios permission', () => {
    const user = AuthUserEntity.fromApi({ ...base, permissionKeys: ['usuarios', 'vendas'] })
    expect(user.ok && user.data.hasPermission('usuarios')).toBe(true)
  })
})
