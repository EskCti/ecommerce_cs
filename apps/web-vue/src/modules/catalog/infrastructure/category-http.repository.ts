import { parseApiError } from '@/shared/parse-api-error'
import { err, ok, type Result } from '@/shared/result'
import { CategoryEntity } from '../domain/category.entity'
import type {
  CategoryInput,
  CategoryListFilter,
  CategoryUpdateInput,
  ICategoryRepository,
} from '../application/category.repository'

type TokenProvider = () => string | null

type CategoryListItemDto = {
  id: string
  name: string
  isActive: boolean
}

type CategoryOutputDto = {
  id: string
  tenantId: number
  name: string
  isActive: boolean
  createdAt: string
  updatedAt: string
}

function mapListItem(row: CategoryListItemDto): Result<CategoryEntity> {
  return CategoryEntity.fromApi({
    id: row.id,
    name: row.name,
    isActive: row.isActive,
  })
}

function mapOutput(row: CategoryOutputDto): Result<CategoryEntity> {
  return CategoryEntity.fromApi({
    id: row.id,
    name: row.name,
    isActive: row.isActive,
    createdAt: row.createdAt,
    updatedAt: row.updatedAt,
  })
}

export class CategoryHttpRepository implements ICategoryRepository {
  constructor(private readonly getToken: TokenProvider) {}

  private headers() {
    const token = this.getToken()
    return {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    }
  }

  async list(filter: CategoryListFilter): Promise<Result<CategoryEntity[]>> {
    const params = new URLSearchParams()
    if (filter.name) params.set('name', filter.name)
    if (filter.isActive !== undefined) params.set('isActive', String(filter.isActive))
    params.set('page', String(filter.page ?? 1))
    params.set('pageSize', String(filter.pageSize ?? 100))

    try {
      const res = await fetch(`/api/catalog/categories?${params}`, { headers: this.headers() })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao listar categorias'))
      const body = (await res.json()) as CategoryListItemDto[]
      const items: CategoryEntity[] = []
      for (const row of body) {
        const mapped = mapListItem(row)
        if (mapped.ok) items.push(mapped.data)
      }
      return ok(items)
    } catch {
      return err('Erro de rede')
    }
  }

  async create(input: CategoryInput): Promise<Result<CategoryEntity>> {
    try {
      const res = await fetch('/api/catalog/categories', {
        method: 'POST',
        headers: this.headers(),
        body: JSON.stringify(input),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao criar categoria'))
      return mapOutput((await res.json()) as CategoryOutputDto)
    } catch {
      return err('Erro de rede')
    }
  }

  async update(id: string, input: CategoryUpdateInput): Promise<Result<CategoryEntity>> {
    try {
      const res = await fetch(`/api/catalog/categories/${id}`, {
        method: 'PUT',
        headers: this.headers(),
        body: JSON.stringify(input),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao atualizar categoria'))
      return mapOutput((await res.json()) as CategoryOutputDto)
    } catch {
      return err('Erro de rede')
    }
  }

  async deactivate(id: string): Promise<Result<void>> {
    try {
      const res = await fetch(`/api/catalog/categories/${id}`, {
        method: 'DELETE',
        headers: this.headers(),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao desativar categoria'))
      return ok(undefined)
    } catch {
      return err('Erro de rede')
    }
  }
}
