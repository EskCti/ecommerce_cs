import type { PersonType, SupplierEntity } from '../domain/supplier.entity'
import type { Result } from '@/shared/result'

export type SupplierInput = {
  name: string
  personType: PersonType
  taxDocument: string
  phone?: string
  email?: string
  address?: string
}

export type SupplierListFilter = {
  name?: string
  personType?: PersonType
  page?: number
  pageSize?: number
}

export type SupplierListPage = {
  items: SupplierEntity[]
  total: number
}

export interface ISupplierRepository {
  list(filter: SupplierListFilter): Promise<Result<SupplierListPage>>
  create(input: SupplierInput): Promise<Result<SupplierEntity>>
  update(id: string, input: Partial<SupplierInput>): Promise<Result<SupplierEntity>>
  deactivate(id: string): Promise<Result<void>>
}
