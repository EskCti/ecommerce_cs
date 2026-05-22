import { UsersPermissionsHttpRepository } from './infrastructure/users-http.repository'
import { AssignPermissionsUseCase } from './application/assign-permissions.usecase'
import { ListPermissionCatalogUseCase } from './application/list-permission-catalog.usecase'
import { ListUsersUseCase } from './application/list-users.usecase'

export function createUsersModule(getToken: () => string | null) {
  const repository = new UsersPermissionsHttpRepository(getToken)
  return {
    repository,
    listUsersUseCase: new ListUsersUseCase(repository),
    listPermissionCatalogUseCase: new ListPermissionCatalogUseCase(repository),
    assignPermissionsUseCase: new AssignPermissionsUseCase(repository),
  }
}
