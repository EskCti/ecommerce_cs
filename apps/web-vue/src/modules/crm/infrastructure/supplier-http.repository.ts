import { err, ok, type Result } from '@/shared/result'
import { SupplierEntity, type PersonType } from '../domain/supplier.entity'
import type {
  ISupplierRepository,
  SupplierInput,
  SupplierListFilter,
  SupplierListPage,
} from '../application/supplier.repository'

type TokenProvider = () => string | null

type SupplierDto = {
  id: string
  name: string
  personType: PersonType
  taxDocument: string
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

export class SupplierHttpRepository implements ISupplierRepository {
  constructor(private readonly getToken: TokenProvider) {}

  private headers() {
    const token = this.getToken()
    return {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    }
  }

  async list(filter: SupplierListFilter): Promise<Result<SupplierListPage>> {
    const params = new URLSearchParams()
    if (filter.name) params.set('name', filter.name)
    if (filter.personType) params.set('personType', filter.personType)
    params.set('page', String(filter.page ?? 1))
    params.set('pageSize', String(filter.pageSize ?? 20))

    try {
      const res = await fetch(`/api/crm/suppliers?${params}`, { headers: this.headers() })
      if (!res.ok) return err(await parseError(res, 'Falha ao listar fornecedores'))
      const body = (await res.json()) as { items: SupplierDto[]; total: number }
      const items: SupplierEntity[] = []
      for (const row of body.items) {
        const mapped = SupplierEntity.fromApi(row)
        if (mapped.ok) items.push(mapped.data)
      }
      return ok({ items, total: body.total })
    } catch {
      return err('Erro de rede')
    }
  }

  async create(input: SupplierInput): Promise<Result<SupplierEntity>> {
    try {
      const res = await fetch('/api/crm/suppliers', {
        method: 'POST',
        headers: this.headers(),
        body: JSON.stringify(input),
      })
      if (!res.ok) return err(await parseError(res, 'Falha ao criar fornecedor'))
      return SupplierEntity.fromApi((await res.json()) as SupplierDto)
    } catch {
      return err('Erro de rede')
    }
  }

  async update(id: string, input: Partial<SupplierInput>): Promise<Result<SupplierEntity>> {
    try {
      const res = await fetch(`/api/crm/suppliers/${id}`, {
        method: 'PUT',
        headers: this.headers(),
        body: JSON.stringify(input),
      })
      if (!res.ok) return err(await parseError(res, 'Falha ao atualizar fornecedor'))
      return SupplierEntity.fromApi((await res.json()) as SupplierDto)
    } catch {
      return err('Erro de rede')
    }
  }

  async deactivate(id: string): Promise<Result<void>> {
    try {
      const res = await fetch(`/api/crm/suppliers/${id}`, {
        method: 'DELETE',
        headers: this.headers(),
      })
      if (!res.ok) return err(await parseError(res, 'Falha ao desativar fornecedor'))
      return ok(undefined)
    } catch {
      return err('Erro de rede')
    }
  }
}
