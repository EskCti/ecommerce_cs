import type { Result } from '@/shared/result'
import type { PermissionCatalogItem } from '../domain/permission-catalog-item'
import type { TenantUserEntity } from '../domain/tenant-user.entity'

export interface IUsersPermissionsRepository {
  listUsers(): Promise<Result<TenantUserEntity[]>>
  listPermissionCatalog(): Promise<Result<PermissionCatalogItem[]>>
  assignPermissions(userId: string, permissionKeys: string[]): Promise<Result<void>>
}
