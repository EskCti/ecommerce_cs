import type { Result } from '@/shared/result'
import type {
  CashFlowData,
  CommissionData,
  PayableData,
  PaymentStatus,
  ReceivableData,
} from '../domain/finance.entity'

export type PagedResult<T> = {
  items: T[]
  total: number
  page: number
  pageSize: number
}

export interface IFinanceRepository {
  listReceivables(params: {
    status?: PaymentStatus
    page?: number
    pageSize?: number
  }): Promise<Result<PagedResult<ReceivableData>>>

  createReceivable(input: {
    description: string
    amount: number
    dueDate: string
  }): Promise<Result<ReceivableData>>

  settleReceivable(id: string, settlementDate: string): Promise<Result<ReceivableData>>

  addReceivableAttachment(id: string, name: string, path: string): Promise<Result<{ id: string; name: string; path: string }>>

  listPayables(params: {
    status?: PaymentStatus
    page?: number
    pageSize?: number
  }): Promise<Result<PagedResult<PayableData>>>

  createPayable(input: {
    description: string
    amount: number
    dueDate: string
    recurrenceDays?: number
  }): Promise<Result<PayableData>>

  settlePayable(id: string, settlementDate: string): Promise<Result<PayableData>>

  listPurchases(params: {
    status?: PaymentStatus
    page?: number
    pageSize?: number
  }): Promise<Result<PagedResult<PayableData>>>

  listCommissions(params: {
    isPaid?: boolean
    page?: number
    pageSize?: number
  }): Promise<Result<PagedResult<CommissionData>>>

  payCommission(id: string, settlementDate: string): Promise<Result<CommissionData>>

  payCommissionsBatch(commissionIds: string[], paymentDate: string): Promise<Result<CommissionData[]>>

  getCashFlow(from: string, to: string): Promise<Result<CashFlowData>>
}
