import type { Result } from '@/shared/result'

export type StockMovementInput = {
  productId: string
  quantity: number
  reason: string
  userId: number
}

export type PurchaseStockInput = {
  productId: string
  quantity: number
  unitCost: number
  reason: string
  userId: number
}

export type LowStockProduct = {
  id: string
  barcode: string
  name: string
  stock: number
  stockAlertLevel: number
}

export type StockMovement = {
  id: string
  productId: string
  quantity: number
  reason: string
  userId: number
  type: string
  createdAt: string
}

export interface IStockRepository {
  recordEntry(input: StockMovementInput): Promise<Result<StockMovement>>
  recordExit(input: StockMovementInput): Promise<Result<StockMovement>>
  purchaseStock(input: PurchaseStockInput): Promise<Result<StockMovement>>
  listMovements(params?: { productId?: string; page?: number; pageSize?: number }): Promise<Result<{ items: StockMovement[]; total: number }>>
  listLowStock(): Promise<Result<LowStockProduct[]>>
}
