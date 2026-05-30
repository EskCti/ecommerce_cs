import type {
  IStockRepository,
  LowStockProduct,
  PurchaseStockInput,
  StockMovement,
  StockMovementInput,
} from './stock.repository'
import type { Result } from '@/shared/result'

export class RecordStockEntryUseCase {
  constructor(private readonly repository: IStockRepository) {}
  execute(input: StockMovementInput): Promise<Result<StockMovement>> {
    return this.repository.recordEntry(input)
  }
}

export class RecordStockExitUseCase {
  constructor(private readonly repository: IStockRepository) {}
  execute(input: StockMovementInput): Promise<Result<StockMovement>> {
    return this.repository.recordExit(input)
  }
}

export class PurchaseStockUseCase {
  constructor(private readonly repository: IStockRepository) {}
  execute(input: PurchaseStockInput): Promise<Result<StockMovement>> {
    return this.repository.purchaseStock(input)
  }
}

export class ListLowStockUseCase {
  constructor(private readonly repository: IStockRepository) {}
  execute(): Promise<Result<LowStockProduct[]>> {
    return this.repository.listLowStock()
  }
}

export class ListStockMovementsUseCase {
  constructor(private readonly repository: IStockRepository) {}
  execute(params?: { productId?: string; page?: number; pageSize?: number }) {
    return this.repository.listMovements(params)
  }
}
