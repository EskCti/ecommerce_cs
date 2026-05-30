import { parseApiError } from '@/shared/parse-api-error'
import { err, ok, type Result } from '@/shared/result'
import type {
  IStockRepository,
  LowStockProduct,
  PurchaseStockInput,
  StockMovement,
  StockMovementInput,
} from '../application/stock.repository'

type TokenProvider = () => string | null

type StockMovementDto = {
  id: string
  productId: string
  quantity: number
  reason: string
  userId: number
  type: string
  createdAt: string
}

type LowStockProductDto = {
  id: string
  barcode: string
  name: string
  stock: number
  stockAlertLevel: number
}

function mapMovement(row: StockMovementDto): StockMovement {
  return {
    id: row.id,
    productId: row.productId,
    quantity: row.quantity,
    reason: row.reason,
    userId: row.userId,
    type: row.type,
    createdAt: row.createdAt,
  }
}

export class StockHttpRepository implements IStockRepository {
  constructor(private readonly getToken: TokenProvider) {}

  private headers() {
    const token = this.getToken()
    return {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    }
  }

  async recordEntry(input: StockMovementInput): Promise<Result<StockMovement>> {
    try {
      const res = await fetch('/api/catalog/stock/movements', {
        method: 'POST',
        headers: this.headers(),
        body: JSON.stringify(input),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao registrar entrada de estoque'))
      return ok(mapMovement((await res.json()) as StockMovementDto))
    } catch {
      return err('Erro de rede')
    }
  }

  async recordExit(input: StockMovementInput): Promise<Result<StockMovement>> {
    try {
      const res = await fetch('/api/catalog/stock/movements/exit', {
        method: 'POST',
        headers: this.headers(),
        body: JSON.stringify(input),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao registrar saída de estoque'))
      return ok(mapMovement((await res.json()) as StockMovementDto))
    } catch {
      return err('Erro de rede')
    }
  }

  async purchaseStock(input: PurchaseStockInput): Promise<Result<StockMovement>> {
    try {
      const res = await fetch('/api/catalog/stock/purchase', {
        method: 'POST',
        headers: this.headers(),
        body: JSON.stringify(input),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao registrar compra de estoque'))
      return ok(mapMovement((await res.json()) as StockMovementDto))
    } catch {
      return err('Erro de rede')
    }
  }

  async listMovements(params?: {
    productId?: string
    page?: number
    pageSize?: number
  }): Promise<Result<{ items: StockMovement[]; total: number }>> {
    const q = new URLSearchParams()
    if (params?.productId) q.set('productId', params.productId)
    q.set('page', String(params?.page ?? 1))
    q.set('pageSize', String(params?.pageSize ?? 50))
    try {
      const res = await fetch(`/api/catalog/stock/movements?${q}`, { headers: this.headers() })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao listar movimentações'))
      const body = (await res.json()) as Record<string, unknown>
      const rows = (body.items ?? body) as StockMovementDto[]
      const items = Array.isArray(rows) ? rows.map(mapMovement) : []
      return ok({
        items,
        total: Number(body.total ?? items.length),
      })
    } catch {
      return err('Erro de rede')
    }
  }

  async listLowStock(): Promise<Result<LowStockProduct[]>> {
    try {
      const res = await fetch('/api/catalog/stock/low', { headers: this.headers() })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao listar estoque baixo'))
      const body = (await res.json()) as LowStockProductDto[]
      return ok(
        body.map((row) => ({
          id: row.id,
          barcode: row.barcode,
          name: row.name,
          stock: row.stock,
          stockAlertLevel: row.stockAlertLevel,
        })),
      )
    } catch {
      return err('Erro de rede')
    }
  }
}
