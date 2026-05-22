import { err, ok, type Result } from '@/shared/result'

export type AuthUserData = {
  readonly id: string
  readonly legacyUserId: number
  readonly tenantId: number
  readonly name: string
  readonly email?: string
  readonly userLevel: string
  readonly permissionKeys: readonly string[]
}

export class AuthUserEntity implements AuthUserData {
  readonly id: string
  readonly legacyUserId: number
  readonly tenantId: number
  readonly name: string
  readonly email?: string
  readonly userLevel: string
  readonly permissionKeys: readonly string[]

  private constructor(data: AuthUserData) {
    this.id = data.id
    this.legacyUserId = data.legacyUserId
    this.tenantId = data.tenantId
    this.name = data.name
    this.email = data.email
    this.userLevel = data.userLevel
    this.permissionKeys = data.permissionKeys
  }

  static fromApi(data: AuthUserData): Result<AuthUserEntity> {
    const name = data.name?.trim()
    if (!name) return err('Nome inválido')
    if (!data.id) return err('Id inválido')
    return ok(new AuthUserEntity({ ...data, name }))
  }

  get isSas(): boolean {
    return this.userLevel === 'Sas'
  }

  get isPrivileged(): boolean {
    return this.isSas || this.userLevel === 'Administrador'
  }

  hasPermission(key: string): boolean {
    if (this.isPrivileged) return true
    return this.permissionKeys.includes(key)
  }
}
