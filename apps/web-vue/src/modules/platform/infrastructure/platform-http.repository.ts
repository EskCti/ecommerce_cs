import { err, ok, type Result } from '@/shared/result'
import { CompanyEntity } from '../domain/company.entity'
import type {
  CompanyListFilter,
  CompanyListPage,
  ITrialRepository,
  RegisterTrialInput,
  RegisterTrialResult,
  ICompanyRepository,
} from '../application/platform.repository'

type CompanyDto = {
  id: number
  companyId: string
  name: string
  email?: string
  phone?: string
  cpf?: string
  cnpj?: string
  active: boolean
  trial: boolean
  nextBillingDate?: string
  monthlyFee: number
  contracts?: { id: number; text: string; signedDate: string }[]
}

function mapCompany(dto: CompanyDto): Result<CompanyEntity> {
  return CompanyEntity.fromApi({
    id: dto.id,
    companyId: dto.companyId,
    name: dto.name,
    email: dto.email,
    phone: dto.phone,
    cpf: dto.cpf,
    cnpj: dto.cnpj,
    active: dto.active,
    trial: dto.trial,
    nextBillingDate: dto.nextBillingDate,
    monthlyFee: dto.monthlyFee,
    contracts: dto.contracts,
  })
}

export class TrialHttpRepository implements ITrialRepository {
  async registerTrial(input: RegisterTrialInput): Promise<Result<RegisterTrialResult>> {
    try {
      const res = await fetch('/api/platform/trial', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          companyName: input.companyName,
          adminName: input.adminName,
          adminEmail: input.adminEmail,
          adminPassword: input.adminPassword,
          phone: input.phone,
          cpf: input.cpf,
          cnpj: input.cnpj,
        }),
      })
      if (!res.ok) {
        const body = (await res.json().catch(() => ({}))) as { error?: string; title?: string }
        if (body.error) return err(body.error)
        if (res.status >= 500) return err('Erro interno do servidor. Verifique se a API e o banco estão rodando.')
        return err(body.title ?? 'Falha no cadastro trial')
      }
      return ok((await res.json()) as RegisterTrialResult)
    } catch {
      return err('Erro de rede')
    }
  }
}

export class CompanyHttpRepository implements ICompanyRepository {
  constructor(private readonly getToken: () => string | null) {}

  private headers() {
    const token = this.getToken()
    return {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    }
  }

  async list(filter: CompanyListFilter): Promise<Result<CompanyListPage>> {
    const params = new URLSearchParams()
    if (filter.name) params.set('name', filter.name)
    if (filter.active !== undefined) params.set('active', String(filter.active))
    if (filter.trial !== undefined) params.set('trial', String(filter.trial))
    params.set('page', String(filter.page ?? 1))
    params.set('pageSize', String(filter.pageSize ?? 20))

    try {
      const res = await fetch(`/api/platform/companies?${params}`, { headers: this.headers() })
      if (!res.ok) return err('Falha ao listar empresas')
      const body = (await res.json()) as { items: CompanyDto[]; total: number }
      const items: CompanyEntity[] = []
      for (const dto of body.items) {
        const mapped = mapCompany(dto)
        if (mapped.ok) items.push(mapped.data)
      }
      return ok({ items, total: body.total })
    } catch {
      return err('Erro de rede')
    }
  }

  async getById(id: number): Promise<Result<CompanyEntity>> {
    try {
      const res = await fetch(`/api/platform/companies/${id}`, { headers: this.headers() })
      if (!res.ok) return err('Empresa não encontrada')
      return mapCompany((await res.json()) as CompanyDto)
    } catch {
      return err('Erro de rede')
    }
  }

  async create(): Promise<Result<CompanyEntity>> {
    return err('Use formulário SAS para criar empresa')
  }

  async update(id: number, input: Partial<CompanyEntity>): Promise<Result<CompanyEntity>> {
    try {
      const res = await fetch(`/api/platform/companies/${id}`, {
        method: 'PUT',
        headers: this.headers(),
        body: JSON.stringify({
          name: input.name,
          phone: input.phone,
          email: input.email,
          cpf: input.cpf,
          cnpj: input.cnpj,
          monthlyFee: input.monthlyFee,
          active: input.active,
        }),
      })
      if (!res.ok) return err('Falha ao salvar empresa')
      return mapCompany((await res.json()) as CompanyDto)
    } catch {
      return err('Erro de rede')
    }
  }
}
