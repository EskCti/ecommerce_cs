import { parseApiError } from '@/shared/parse-api-error'
import { err, ok, type Result } from '@/shared/result'
import type {
  CashFlowData,
  CommissionData,
  PayableData,
  PaymentStatus,
  ReceivableData,
} from '../domain/finance.entity'
import type { IFinanceRepository, PagedResult } from '../application/finance.repository'

type TokenProvider = () => string | null

export class FinanceHttpRepository implements IFinanceRepository {
  constructor(private readonly getToken: TokenProvider) {}

  private headers() {
    const token = this.getToken()
    return {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    }
  }

  private async parsePaged<T>(res: Response, fallbackError: string): Promise<Result<PagedResult<T>>> {
    if (!res.ok) return err(await parseApiError(res, fallbackError))
    const body = (await res.json()) as Record<string, unknown>
    const items = (body.items ?? body.Items ?? []) as T[]
    return ok({
      items,
      total: Number(body.total ?? body.Total ?? items.length),
      page: Number(body.page ?? body.Page ?? 1),
      pageSize: Number(body.pageSize ?? body.PageSize ?? 20),
    })
  }

  async listReceivables(params: {
    status?: PaymentStatus
    page?: number
    pageSize?: number
  }): Promise<Result<PagedResult<ReceivableData>>> {
    const q = new URLSearchParams()
    if (params.status) q.set('status', params.status)
    q.set('page', String(params.page ?? 1))
    q.set('pageSize', String(params.pageSize ?? 20))
    try {
      const res = await fetch(`/api/finance/receivables?${q}`, { headers: this.headers() })
      return this.parsePaged<ReceivableData>(res, 'Falha ao listar contas a receber')
    } catch {
      return err('Erro de rede')
    }
  }

  async createReceivable(input: {
    description: string
    amount: number
    dueDate: string
  }): Promise<Result<ReceivableData>> {
    try {
      const res = await fetch('/api/finance/receivables', {
        method: 'POST',
        headers: this.headers(),
        body: JSON.stringify(input),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao criar conta a receber'))
      return ok(await res.json())
    } catch {
      return err('Erro de rede')
    }
  }

  async settleReceivable(id: string, settlementDate: string): Promise<Result<ReceivableData>> {
    try {
      const res = await fetch(`/api/finance/receivables/${id}/settle`, {
        method: 'POST',
        headers: this.headers(),
        body: JSON.stringify({ settlementDate }),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao baixar conta'))
      return ok(await res.json())
    } catch {
      return err('Erro de rede')
    }
  }

  async addReceivableAttachment(
    id: string,
    name: string,
    path: string,
  ): Promise<Result<{ id: string; name: string; path: string }>> {
    try {
      const res = await fetch(`/api/finance/receivables/${id}/attachments`, {
        method: 'POST',
        headers: this.headers(),
        body: JSON.stringify({ name, path }),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao anexar arquivo'))
      return ok(await res.json())
    } catch {
      return err('Erro de rede')
    }
  }

  async listPayables(params: {
    status?: PaymentStatus
    page?: number
    pageSize?: number
  }): Promise<Result<PagedResult<PayableData>>> {
    const q = new URLSearchParams()
    if (params.status) q.set('status', params.status)
    q.set('page', String(params.page ?? 1))
    q.set('pageSize', String(params.pageSize ?? 20))
    try {
      const res = await fetch(`/api/finance/payables?${q}`, { headers: this.headers() })
      return this.parsePaged<PayableData>(res, 'Falha ao listar despesas')
    } catch {
      return err('Erro de rede')
    }
  }

  async createPayable(input: {
    description: string
    amount: number
    dueDate: string
    recurrenceDays?: number
  }): Promise<Result<PayableData>> {
    try {
      const res = await fetch('/api/finance/payables', {
        method: 'POST',
        headers: this.headers(),
        body: JSON.stringify(input),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao criar despesa'))
      return ok(await res.json())
    } catch {
      return err('Erro de rede')
    }
  }

  async settlePayable(id: string, settlementDate: string): Promise<Result<PayableData>> {
    try {
      const res = await fetch(`/api/finance/payables/${id}/settle`, {
        method: 'POST',
        headers: this.headers(),
        body: JSON.stringify({ settlementDate }),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao baixar despesa'))
      return ok(await res.json())
    } catch {
      return err('Erro de rede')
    }
  }

  async listPurchases(params: {
    status?: PaymentStatus
    page?: number
    pageSize?: number
  }): Promise<Result<PagedResult<PayableData>>> {
    const q = new URLSearchParams()
    if (params.status) q.set('status', params.status)
    q.set('page', String(params.page ?? 1))
    q.set('pageSize', String(params.pageSize ?? 20))
    try {
      const res = await fetch(`/api/finance/purchases?${q}`, { headers: this.headers() })
      return this.parsePaged<PayableData>(res, 'Falha ao listar compras')
    } catch {
      return err('Erro de rede')
    }
  }

  async listCommissions(params: {
    isPaid?: boolean
    page?: number
    pageSize?: number
  }): Promise<Result<PagedResult<CommissionData>>> {
    const q = new URLSearchParams()
    if (params.isPaid !== undefined) q.set('isPaid', String(params.isPaid))
    q.set('page', String(params.page ?? 1))
    q.set('pageSize', String(params.pageSize ?? 20))
    try {
      const res = await fetch(`/api/finance/commissions?${q}`, { headers: this.headers() })
      return this.parsePaged<CommissionData>(res, 'Falha ao listar comissões')
    } catch {
      return err('Erro de rede')
    }
  }

  async payCommission(id: string, settlementDate: string): Promise<Result<CommissionData>> {
    try {
      const res = await fetch(`/api/finance/commissions/${id}/pay`, {
        method: 'POST',
        headers: this.headers(),
        body: JSON.stringify({ settlementDate }),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao pagar comissão'))
      return ok(await res.json())
    } catch {
      return err('Erro de rede')
    }
  }

  async payCommissionsBatch(
    commissionIds: string[],
    paymentDate: string,
  ): Promise<Result<CommissionData[]>> {
    try {
      const res = await fetch('/api/finance/commissions/pay/batch', {
        method: 'POST',
        headers: this.headers(),
        body: JSON.stringify({ commissionIds, paymentDate }),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao pagar comissões em lote'))
      return ok(await res.json())
    } catch {
      return err('Erro de rede')
    }
  }

  async getCashFlow(from: string, to: string): Promise<Result<CashFlowData>> {
    const q = new URLSearchParams({ from, to })
    try {
      const res = await fetch(`/api/finance/cash-flow?${q}`, { headers: this.headers() })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao carregar fluxo de caixa'))
      return ok(await res.json())
    } catch {
      return err('Erro de rede')
    }
  }
}
