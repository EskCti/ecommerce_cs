import { RegisterTrialUseCase } from './application/register-trial.usecase'
import { GetCompanyUseCase, ListCompaniesUseCase, UpdateCompanyUseCase } from './application/company.usecase'
import { CompanyHttpRepository, TrialHttpRepository } from './infrastructure/platform-http.repository'

export function createPlatformModule(getToken: () => string | null) {
  const trialRepo = new TrialHttpRepository()
  const companyRepo = new CompanyHttpRepository(getToken)
  return {
    registerTrialUseCase: new RegisterTrialUseCase(trialRepo),
    listCompaniesUseCase: new ListCompaniesUseCase(companyRepo),
    getCompanyUseCase: new GetCompanyUseCase(companyRepo),
    updateCompanyUseCase: new UpdateCompanyUseCase(companyRepo),
  }
}
