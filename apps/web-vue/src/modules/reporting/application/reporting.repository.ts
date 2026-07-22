import type { ReportFilterInput, ReportKind } from '../domain/report.entity'

export interface IReportingRepository {
  downloadReport(kind: ReportKind, filter: ReportFilterInput): Promise<Result<Blob>>
  downloadReceipt(saleId: string): Promise<Result<Blob>>
}

export type Result<T> = { ok: true; data: T } | { ok: false; error: string }
