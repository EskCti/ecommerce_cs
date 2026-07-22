import type { Result } from '@/shared/result'
import type { ExchangeEntity, ExchangeListItemData, RegisterExchangeFormInput } from '../domain/exchange.entity'

export type ExchangeListFilter = {
  from?: string
  to?: string
  customerId?: string
  productId?: string
  page?: number
  pageSize?: number
}

export type ExchangeListPage = {
  items: ExchangeListItemData[]
  page: number
  pageSize: number
  totalCount: number
}

export interface IReturnsRepository {
  registerExchange(input: RegisterExchangeFormInput): Promise<Result<ExchangeEntity>>
  listExchanges(filter: ExchangeListFilter): Promise<Result<ExchangeListPage>>
  deleteExchange(id: string): Promise<Result<ExchangeEntity>>
}
