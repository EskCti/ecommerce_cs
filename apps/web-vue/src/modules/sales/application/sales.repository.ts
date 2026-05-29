import type { Result } from '@/shared/result'
import type { CashSessionEntity } from '../domain/cash-session.entity'
import type { SaleEntity, SaleListPageEntity } from '../domain/sale.entity'

export type OpenCashSessionInput = {
  terminalId: string
  managerUserId: string
  managerPin: string
  openingFloat: number
}

export type AddCartItemInput = {
  scannedValue: string
  unitPriceOverride?: number
}

export type ConfirmGradeInput = {
  lineId: string
  gradeOptionIds?: string[]
  gradeVariantId?: string
}

export type CashWithdrawalInput = {
  amount: number
}

export type CloseCashSessionInput = {
  managerUserId: string
  managerPin: string
  countedCash: number
}

export type FinalizeSaleInput = {
  paymentMethodId: string
  paymentTerms: 'Cash' | 'Credit'
  customerId?: string
  amountPaid: number
  discountAmount?: number
  sellerCommissionPercent?: number
}

export type SaleListFilter = {
  from?: string
  to?: string
  operatorUserId?: string
  page?: number
  pageSize?: number
}

export interface ISalesRepository {
  openSession(input: OpenCashSessionInput): Promise<Result<CashSessionEntity>>
  getCurrentSession(): Promise<Result<CashSessionEntity | null>>
  addItem(input: AddCartItemInput): Promise<Result<CashSessionEntity>>
  confirmGrade(input: ConfirmGradeInput): Promise<Result<CashSessionEntity>>
  removeItem(lineId: string): Promise<Result<CashSessionEntity>>
  withdrawal(input: CashWithdrawalInput): Promise<Result<CashSessionEntity>>
  closeSession(input: CloseCashSessionInput): Promise<Result<CashSessionEntity>>
  finalizeSale(input: FinalizeSaleInput): Promise<Result<SaleEntity>>
  listSales(filter: SaleListFilter): Promise<Result<SaleListPageEntity>>
  cancelSale(saleId: string): Promise<Result<SaleEntity>>
}
