import type { Result } from '@/shared/result'
import type { TenantUserEntity } from '../domain/tenant-user.entity'
import type { IUsersPermissionsRepository } from './users.repository'

export class ListUsersUseCase {
  constructor(private readonly repository: IUsersPermissionsRepository) {}

  execute(): Promise<Result<TenantUserEntity[]>> {
    return this.repository.listUsers()
  }
}
