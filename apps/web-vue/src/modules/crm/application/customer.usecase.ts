import { ok, type Result } from '@/shared/result'
import type {
  CustomerInput,
  CustomerListFilter,
  CustomerListPage,
  FindOrCreateByCpfInput,
  ICustomerRepository,
} from './customer.repository'
import type { CustomerEntity } from '../domain/customer.entity'

export class ListCustomersUseCase {
  constructor(private readonly repository: ICustomerRepository) {}
  execute(filter: CustomerListFilter): Promise<Result<CustomerListPage>> {
    return this.repository.list(filter)
  }
}

export class CreateCustomerUseCase {
  constructor(private readonly repository: ICustomerRepository) {}
  execute(input: CustomerInput): Promise<Result<CustomerEntity>> {
    return this.repository.create(input)
  }
}

export class UpdateCustomerUseCase {
  constructor(private readonly repository: ICustomerRepository) {}
  execute(id: string, input: Partial<CustomerInput>): Promise<Result<CustomerEntity>> {
    return this.repository.update(id, input)
  }
}

export class DeactivateCustomerUseCase {
  constructor(private readonly repository: ICustomerRepository) {}
  async execute(id: string): Promise<Result<void>> {
    const result = await this.repository.deactivate(id)
    if (!result.ok) return result
    return ok(undefined)
  }
}

export class FindOrCreateByCpfUseCase {
  constructor(private readonly repository: ICustomerRepository) {}
  execute(input: FindOrCreateByCpfInput): Promise<Result<CustomerEntity>> {
    return this.repository.findOrCreateByCpf(input)
  }
}
