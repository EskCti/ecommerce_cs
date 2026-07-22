import { DownloadReceiptUseCase, DownloadReportUseCase } from './application/reporting.usecase'
import { ReportingHttpRepository } from './infrastructure/reporting-http.repository'

export function createReportingModule(getToken: () => string | null) {
  const repository = new ReportingHttpRepository(getToken)

  return {
    downloadReportUseCase: new DownloadReportUseCase(repository),
    downloadReceiptUseCase: new DownloadReceiptUseCase(repository),
  }
}
