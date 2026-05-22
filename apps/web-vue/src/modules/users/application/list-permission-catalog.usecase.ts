import type { Result } from '@/shared/result'
import type { PermissionCatalogItem } from '../domain/permission-catalog-item'
import type { IUsersPermissionsRepository } from './users.repository'

export class ListPermissionCatalogUseCase {
  constructor(private readonly repository: IUsersPermissionsRepository) {}

  execute(): Promise<Result<PermissionCatalogItem[]>> {
    return this.repository.listPermissionCatalog()
  }
}
