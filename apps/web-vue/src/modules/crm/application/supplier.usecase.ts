import { ok, type Result } from '@/shared/result'
import type {
  ISupplierRepository,
  SupplierInput,
  SupplierListFilter,
  SupplierListPage,
} from './supplier.repository'
import type { SupplierEntity } from '../domain/supplier.entity'

export class ListSuppliersUseCase {
  constructor(private readonly repository: ISupplierRepository) {}
  execute(filter: SupplierListFilter): Promise<Result<SupplierListPage>> {
    return this.repository.list(filter)
  }
}

export class CreateSupplierUseCase {
  constructor(private readonly repository: ISupplierRepository) {}
  execute(input: SupplierInput): Promise<Result<SupplierEntity>> {
    return this.repository.create(input)
  }
}

export class UpdateSupplierUseCase {
  constructor(private readonly repository: ISupplierRepository) {}
  execute(id: string, input: Partial<SupplierInput>): Promise<Result<SupplierEntity>> {
    return this.repository.update(id, input)
  }
}

export class DeactivateSupplierUseCase {
  constructor(private readonly repository: ISupplierRepository) {}
  async execute(id: string): Promise<Result<void>> {
    const result = await this.repository.deactivate(id)
    if (!result.ok) return result
    return ok(undefined)
  }
}
