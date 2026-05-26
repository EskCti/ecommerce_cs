import { err, ok, type Result } from '@/shared/result'
import { CustomerEntity } from '../domain/customer.entity'
import type {
  CustomerInput,
  CustomerListFilter,
  CustomerListPage,
  FindOrCreateByCpfInput,
  ICustomerRepository,
} from '../application/customer.repository'

type TokenProvider = () => string | null

type CustomerDto = {
  id: string
  name: string
  cpf: string
  phone?: string
  email?: string
  address?: string
  isActive: boolean
  createdAt: string
  updatedAt: string
}

async function parseError(res: Response, fallback: string): Promise<string> {
  try {
    const body = (await res.json()) as { error?: string; title?: string }
    return body.error ?? body.title ?? fallback
  } catch {
    return fallback
  }
}

export class CustomerHttpRepository implements ICustomerRepository {
  constructor(private readonly getToken: TokenProvider) {}

  private headers() {
    const token = this.getToken()
    return {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    }
  }

  async list(filter: CustomerListFilter): Promise<Result<CustomerListPage>> {
    const params = new URLSearchParams()
    if (filter.name) params.set('name', filter.name)
    if (filter.cpf) params.set('cpf', filter.cpf)
    params.set('page', String(filter.page ?? 1))
    params.set('pageSize', String(filter.pageSize ?? 20))

    try {
      const res = await fetch(`/api/crm/customers?${params}`, { headers: this.headers() })
      if (!res.ok) return err(await parseError(res, 'Falha ao listar clientes'))
      const body = (await res.json()) as { items: CustomerDto[]; total: number }
      const items: CustomerEntity[] = []
      for (const row of body.items) {
        const mapped = CustomerEntity.fromApi(row)
        if (mapped.ok) items.push(mapped.data)
      }
      return ok({ items, total: body.total })
    } catch {
      return err('Erro de rede')
    }
  }

  async create(input: CustomerInput): Promise<Result<CustomerEntity>> {
    try {
      const res = await fetch('/api/crm/customers', {
        method: 'POST',
        headers: this.headers(),
        body: JSON.stringify(input),
      })
      if (!res.ok) return err(await parseError(res, 'Falha ao criar cliente'))
      return CustomerEntity.fromApi((await res.json()) as CustomerDto)
    } catch {
      return err('Erro de rede')
    }
  }

  async update(id: string, input: Partial<CustomerInput>): Promise<Result<CustomerEntity>> {
    try {
      const res = await fetch(`/api/crm/customers/${id}`, {
        method: 'PUT',
        headers: this.headers(),
        body: JSON.stringify(input),
      })
      if (!res.ok) return err(await parseError(res, 'Falha ao atualizar cliente'))
      return CustomerEntity.fromApi((await res.json()) as CustomerDto)
    } catch {
      return err('Erro de rede')
    }
  }

  async deactivate(id: string): Promise<Result<void>> {
    try {
      const res = await fetch(`/api/crm/customers/${id}`, {
        method: 'DELETE',
        headers: this.headers(),
      })
      if (!res.ok) return err(await parseError(res, 'Falha ao desativar cliente'))
      return ok(undefined)
    } catch {
      return err('Erro de rede')
    }
  }

  async findOrCreateByCpf(input: FindOrCreateByCpfInput): Promise<Result<CustomerEntity>> {
    try {
      const res = await fetch('/api/crm/customers/find-or-create-by-cpf', {
        method: 'POST',
        headers: this.headers(),
        body: JSON.stringify(input),
      })
      if (!res.ok) return err(await parseError(res, 'Falha ao buscar ou criar cliente por CPF'))
      return CustomerEntity.fromApi((await res.json()) as CustomerDto)
    } catch {
      return err('Erro de rede')
    }
  }
}
