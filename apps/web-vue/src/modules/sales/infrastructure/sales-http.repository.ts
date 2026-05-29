import { parseApiError } from '@/shared/parse-api-error'
import { err, ok, type Result } from '@/shared/result'
import { CashSessionEntity, type CashSessionData } from '../domain/cash-session.entity'
import { SaleEntity, SaleListPageEntity, type SaleData, type SaleListPageData } from '../domain/sale.entity'
import type {
  AddCartItemInput,
  CashWithdrawalInput,
  CloseCashSessionInput,
  ConfirmGradeInput,
  FinalizeSaleInput,
  ISalesRepository,
  OpenCashSessionInput,
  SaleListFilter,
} from '../application/sales.repository'

type TokenProvider = () => string | null

export class SalesHttpRepository implements ISalesRepository {
  constructor(private readonly getToken: TokenProvider) {}

  private headers() {
    const token = this.getToken()
    return {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    }
  }

  private async mapSession(body: CashSessionData): Promise<Result<CashSessionEntity>> {
    return CashSessionEntity.fromApi(body)
  }

  async openSession(input: OpenCashSessionInput): Promise<Result<CashSessionEntity>> {
    try {
      const res = await fetch('/api/sales/cash-session/open', {
        method: 'POST',
        headers: this.headers(),
        body: JSON.stringify(input),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao abrir caixa'))
      return this.mapSession((await res.json()) as CashSessionData)
    } catch {
      return err('Erro de rede')
    }
  }

  async getCurrentSession(): Promise<Result<CashSessionEntity | null>> {
    try {
      const res = await fetch('/api/sales/cash-session/current', { headers: this.headers() })
      if (res.status === 404) return ok(null)
      if (!res.ok) return err(await parseApiError(res, 'Falha ao buscar sessão de caixa'))
      return this.mapSession((await res.json()) as CashSessionData)
    } catch {
      return err('Erro de rede')
    }
  }

  async addItem(input: AddCartItemInput): Promise<Result<CashSessionEntity>> {
    try {
      const res = await fetch('/api/sales/cash-session/current/cart/items', {
        method: 'POST',
        headers: this.headers(),
        body: JSON.stringify(input),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao adicionar item'))
      return this.mapSession((await res.json()) as CashSessionData)
    } catch {
      return err('Erro de rede')
    }
  }

  async confirmGrade(input: ConfirmGradeInput): Promise<Result<CashSessionEntity>> {
    try {
      const res = await fetch(
        `/api/sales/cash-session/current/cart/items/${input.lineId}/confirm-grade`,
        {
          method: 'POST',
          headers: this.headers(),
          body: JSON.stringify({
            gradeOptionIds: input.gradeOptionIds ?? [],
            gradeVariantId: input.gradeVariantId ?? null,
          }),
        },
      )
      if (!res.ok) return err(await parseApiError(res, 'Falha ao confirmar grade'))
      return this.mapSession((await res.json()) as CashSessionData)
    } catch {
      return err('Erro de rede')
    }
  }

  async removeItem(lineId: string): Promise<Result<CashSessionEntity>> {
    try {
      const res = await fetch(`/api/sales/cash-session/current/cart/items/${lineId}`, {
        method: 'DELETE',
        headers: this.headers(),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao remover item'))
      return this.mapSession((await res.json()) as CashSessionData)
    } catch {
      return err('Erro de rede')
    }
  }

  async withdrawal(input: CashWithdrawalInput): Promise<Result<CashSessionEntity>> {
    try {
      const res = await fetch('/api/sales/cash-session/withdrawal', {
        method: 'POST',
        headers: this.headers(),
        body: JSON.stringify(input),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao registrar sangria'))
      return this.mapSession((await res.json()) as CashSessionData)
    } catch {
      return err('Erro de rede')
    }
  }

  async closeSession(input: CloseCashSessionInput): Promise<Result<CashSessionEntity>> {
    try {
      const res = await fetch('/api/sales/cash-session/close', {
        method: 'POST',
        headers: this.headers(),
        body: JSON.stringify(input),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao fechar caixa'))
      return this.mapSession((await res.json()) as CashSessionData)
    } catch {
      return err('Erro de rede')
    }
  }

  async finalizeSale(input: FinalizeSaleInput): Promise<Result<SaleEntity>> {
    try {
      const res = await fetch('/api/sales/cash-session/finalize', {
        method: 'POST',
        headers: this.headers(),
        body: JSON.stringify(input),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao finalizar venda'))
      return SaleEntity.fromApi((await res.json()) as SaleData)
    } catch {
      return err('Erro de rede')
    }
  }

  async listSales(filter: SaleListFilter): Promise<Result<SaleListPageEntity>> {
    const params = new URLSearchParams()
    if (filter.from) params.set('from', filter.from)
    if (filter.to) params.set('to', filter.to)
    if (filter.operatorUserId) params.set('operatorUserId', filter.operatorUserId)
    params.set('page', String(filter.page ?? 1))
    params.set('pageSize', String(filter.pageSize ?? 20))

    try {
      const res = await fetch(`/api/sales?${params}`, { headers: this.headers() })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao listar vendas'))
      return SaleListPageEntity.fromApi((await res.json()) as SaleListPageData)
    } catch {
      return err('Erro de rede')
    }
  }

  async cancelSale(saleId: string): Promise<Result<SaleEntity>> {
    try {
      const res = await fetch(`/api/sales/${saleId}/cancel`, {
        method: 'POST',
        headers: this.headers(),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao cancelar venda'))
      return SaleEntity.fromApi((await res.json()) as SaleData)
    } catch {
      return err('Erro de rede')
    }
  }
}
