import { err, type Result } from '@/shared/result'
import type { CompanyEntity } from '../domain/company.entity'
import type { CompanyListFilter, CompanyListPage, ICompanyRepository } from './platform.repository'

export class ListCompaniesUseCase {
  constructor(private readonly repo: ICompanyRepository) {}

  execute(filter: CompanyListFilter): Promise<Result<CompanyListPage>> {
    return this.repo.list(filter)
  }
}

export class GetCompanyUseCase {
  constructor(private readonly repo: ICompanyRepository) {}

  execute(id: number): Promise<Result<CompanyEntity>> {
    if (!id) return Promise.resolve(err('Empresa inválida'))
    return this.repo.getById(id)
  }
}

export class UpdateCompanyUseCase {
  constructor(private readonly repo: ICompanyRepository) {}

  execute(id: number, input: Partial<CompanyEntity>): Promise<Result<CompanyEntity>> {
    if (!id) return Promise.resolve(err('Empresa inválida'))
    return this.repo.update(id, input)
  }
}
