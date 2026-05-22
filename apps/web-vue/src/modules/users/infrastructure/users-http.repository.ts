import { err, ok, type Result } from '@/shared/result'
import type { IUsersPermissionsRepository } from '../application/users.repository'
import type { PermissionCatalogItem } from '../domain/permission-catalog-item'
import { TenantUserEntity } from '../domain/tenant-user.entity'

type UserListItemDto = {
  id: string
  legacyUserId: number
  name: string
  email?: string
  userLevel: string
  permissionKeys: string[]
}

export class UsersPermissionsHttpRepository implements IUsersPermissionsRepository {
  constructor(private readonly getToken: () => string | null) {}

  private async request<T>(path: string, init?: RequestInit): Promise<Result<T>> {
    const token = this.getToken()
    if (!token) return err('Sessão expirada')

    try {
      const res = await fetch(path, {
        ...init,
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
          ...(init?.headers ?? {}),
        },
      })
      if (!res.ok) {
        const body = (await res.json().catch(() => ({}))) as { error?: string }
        return err(body.error ?? 'Requisição falhou')
      }
      if (res.status === 204) return ok(undefined as T)
      return ok((await res.json()) as T)
    } catch {
      return err('Erro de rede')
    }
  }

  async listUsers(): Promise<Result<TenantUserEntity[]>> {
    const result = await this.request<UserListItemDto[]>('/api/users')
    if (!result.ok) return result
    const users: TenantUserEntity[] = []
    for (const row of result.data) {
      const mapped = TenantUserEntity.fromApi(row)
      if (!mapped.ok) return mapped
      users.push(mapped.data)
    }
    return ok(users)
  }

  async listPermissionCatalog(): Promise<Result<PermissionCatalogItem[]>> {
    return this.request<PermissionCatalogItem[]>('/api/permissions/catalog')
  }

  async assignPermissions(userId: string, permissionKeys: string[]): Promise<Result<void>> {
    return this.request<void>(`/api/users/${userId}/permissions`, {
      method: 'PUT',
      body: JSON.stringify({ permissionKeys }),
    })
  }
}
