import { err, type Result } from '@/shared/result'
import type { IUsersPermissionsRepository } from './users.repository'

export class AssignPermissionsUseCase {
  constructor(private readonly repository: IUsersPermissionsRepository) {}

  execute(userId: string, permissionKeys: string[]): Promise<Result<void>> {
    if (!userId) return Promise.resolve(err('Selecione um usuário'))
    if (permissionKeys.length === 0) return Promise.resolve(err('Selecione ao menos uma permissão'))
    return this.repository.assignPermissions(userId, permissionKeys)
  }
}
