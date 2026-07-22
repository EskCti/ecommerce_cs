import type { ReportFilterInput, ReportKind } from '../domain/report.entity'
import type { IReportingRepository, Result } from '../application/reporting.repository'

function buildQuery(filter: ReportFilterInput): string {
  const params = new URLSearchParams()
  if (filter.from) params.set('from', filter.from)
  if (filter.to) params.set('to', filter.to)
  if (filter.customerId) params.set('customerId', filter.customerId)
  if (filter.sellerId) params.set('sellerId', filter.sellerId)
  if (filter.status) params.set('status', filter.status)
  const query = params.toString()
  return query ? `?${query}` : ''
}

export class ReportingHttpRepository implements IReportingRepository {
  constructor(private readonly getToken: () => string | null) {}

  async downloadReport(kind: ReportKind, filter: ReportFilterInput): Promise<Result<Blob>> {
    const path = `/api/reporting/${kind}${buildQuery(filter)}`
    return this.fetchBlob(path)
  }

  async downloadReceipt(saleId: string): Promise<Result<Blob>> {
    return this.fetchBlob(`/api/reporting/receipts/${saleId}`)
  }

  private async fetchBlob(path: string): Promise<Result<Blob>> {
    const token = this.getToken()
    if (!token) return { ok: false, error: 'Sessão expirada' }

    const response = await fetch(path, {
      headers: { Authorization: `Bearer ${token}` },
    })

    if (!response.ok) {
      let message = `Erro ${response.status}`
      try {
        const body = (await response.json()) as { error?: string }
        if (body.error) message = body.error
      } catch {
        /* ignore */
      }
      return { ok: false, error: message }
    }

    const blob = await response.blob()
    return { ok: true, data: blob }
  }
}
