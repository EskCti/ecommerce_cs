import { err, ok, type Result } from '@/shared/result'
import {
  ExchangeEntity,
  type ExchangeData,
  type ExchangeListItemData,
  type RegisterExchangeFormInput,
} from '../domain/exchange.entity'
import type { ExchangeListFilter, ExchangeListPage, IReturnsRepository } from '../application/returns.repository'

type ApiExchange = ExchangeData

type ApiExchangeListPage = {
  items: ExchangeListItemData[]
  page: number
  pageSize: number
  totalCount: number
}

export class ReturnsHttpRepository implements IReturnsRepository {
  constructor(private readonly getToken: () => string | null) {}

  private async request<T>(path: string, init?: RequestInit): Promise<Result<T>> {
    const token = this.getToken()
    if (!token) return err('Sessão expirada')

    const response = await fetch(`/api/returns/exchanges${path}`, {
      ...init,
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`,
        ...(init?.headers ?? {}),
      },
    })

    if (!response.ok) {
      const body = (await response.json().catch(() => null)) as { error?: string } | null
      return err(body?.error ?? `Erro HTTP ${response.status}`)
    }

    if (response.status === 204) return ok(undefined as T)
    return ok((await response.json()) as T)
  }

  async registerExchange(input: RegisterExchangeFormInput): Promise<Result<ExchangeEntity>> {
    const result = await this.request<ApiExchange>('', {
      method: 'POST',
      body: JSON.stringify({
        customerId: input.customerId || null,
        cpf: input.cpf || null,
        customerName: input.customerName || null,
        productInId: input.productInId,
        productOutId: input.productOutId,
        gradeVariantInId: input.gradeVariantInId || null,
        gradeVariantOutId: input.gradeVariantOutId || null,
        gradeOptionIdsIn: input.gradeOptionIdsIn ?? [],
        gradeOptionIdsOut: input.gradeOptionIdsOut ?? [],
      }),
    })
    if (!result.ok) return result
    return ExchangeEntity.fromApi(result.data)
  }

  async listExchanges(filter: ExchangeListFilter): Promise<Result<ExchangeListPage>> {
    const params = new URLSearchParams()
    if (filter.from) params.set('from', filter.from)
    if (filter.to) params.set('to', filter.to)
    if (filter.customerId) params.set('customerId', filter.customerId)
    if (filter.productId) params.set('productId', filter.productId)
    if (filter.page) params.set('page', String(filter.page))
    if (filter.pageSize) params.set('pageSize', String(filter.pageSize))

    const query = params.toString()
    const result = await this.request<ApiExchangeListPage>(query ? `?${query}` : '')
    if (!result.ok) return result
    return ok(result.data)
  }

  async deleteExchange(id: string): Promise<Result<ExchangeEntity>> {
    const result = await this.request<ApiExchange>(`/${id}`, { method: 'DELETE' })
    if (!result.ok) return result
    return ExchangeEntity.fromApi(result.data)
  }
}
