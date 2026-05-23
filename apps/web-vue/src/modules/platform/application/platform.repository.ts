import type { Result } from '@/shared/result'
import type { CompanyEntity } from '../domain/company.entity'

export type RegisterTrialInput = {
  companyName: string
  adminName: string
  adminEmail: string
  adminPassword: string
  phone?: string
  cpf?: string
  cnpj?: string
}

export type RegisterTrialResult = {
  tenantId: number
  companyId: string
  adminUserId: string
}

export type CompanyListFilter = {
  name?: string
  active?: boolean
  trial?: boolean
  page?: number
  pageSize?: number
}

export type CompanyListPage = {
  items: CompanyEntity[]
  total: number
}

export interface ITrialRepository {
  registerTrial(input: RegisterTrialInput): Promise<Result<RegisterTrialResult>>
}

export interface ICompanyRepository {
  list(filter: CompanyListFilter): Promise<Result<CompanyListPage>>
  getById(id: number): Promise<Result<CompanyEntity>>
  create(input: Omit<CompanyEntity, 'id' | 'companyId' | 'contracts'> & { trial: boolean }): Promise<Result<CompanyEntity>>
  update(id: number, input: Partial<CompanyEntity>): Promise<Result<CompanyEntity>>
}
