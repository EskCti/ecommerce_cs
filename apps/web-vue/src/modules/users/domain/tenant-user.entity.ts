import { err, ok, type Result } from '@/shared/result'

export type TenantUserData = {
  readonly id: string
  readonly legacyUserId: number
  readonly name: string
  readonly email?: string
  readonly userLevel: string
  readonly permissionKeys: readonly string[]
}

export class TenantUserEntity implements TenantUserData {
  readonly id: string
  readonly legacyUserId: number
  readonly name: string
  readonly email?: string
  readonly userLevel: string
  readonly permissionKeys: readonly string[]

  private constructor(data: TenantUserData) {
    this.id = data.id
    this.legacyUserId = data.legacyUserId
    this.name = data.name
    this.email = data.email
    this.userLevel = data.userLevel
    this.permissionKeys = data.permissionKeys
  }

  static fromApi(data: TenantUserData): Result<TenantUserEntity> {
    if (!data.id) return err('Id inválido')
    const name = data.name?.trim()
    if (!name) return err('Nome inválido')
    return ok(new TenantUserEntity({ ...data, name }))
  }
}
