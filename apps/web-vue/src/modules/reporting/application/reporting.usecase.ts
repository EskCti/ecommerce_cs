import type { ReportFilterInput, ReportKind } from '../domain/report.entity'
import type { IReportingRepository, Result } from './reporting.repository'

export class DownloadReportUseCase {
  constructor(private readonly repository: IReportingRepository) {}

  execute(kind: ReportKind, filter: ReportFilterInput): Promise<Result<Blob>> {
    return this.repository.downloadReport(kind, filter)
  }
}

export class DownloadReceiptUseCase {
  constructor(private readonly repository: IReportingRepository) {}

  execute(saleId: string): Promise<Result<Blob>> {
    return this.repository.downloadReceipt(saleId)
  }
}

export function triggerBlobDownload(blob: Blob, fileName: string): void {
  const url = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = url
  anchor.download = fileName
  anchor.click()
  URL.revokeObjectURL(url)
}
