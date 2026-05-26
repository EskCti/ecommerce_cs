import type { CustomerEntity } from '../domain/customer.entity'
import type { Result } from '@/shared/result'

export type CustomerInput = {
  name: string
  cpf: string
  phone?: string
  email?: string
  address?: string
}

export type FindOrCreateByCpfInput = {
  cpf: string
  name: string
  phone?: string
  email?: string
  address?: string
}

export type CustomerListFilter = {
  name?: string
  cpf?: string
  page?: number
  pageSize?: number
}

export type CustomerListPage = {
  items: CustomerEntity[]
  total: number
}

export interface ICustomerRepository {
  list(filter: CustomerListFilter): Promise<Result<CustomerListPage>>
  create(input: CustomerInput): Promise<Result<CustomerEntity>>
  update(id: string, input: Partial<CustomerInput>): Promise<Result<CustomerEntity>>
  deactivate(id: string): Promise<Result<void>>
  findOrCreateByCpf(input: FindOrCreateByCpfInput): Promise<Result<CustomerEntity>>
}
