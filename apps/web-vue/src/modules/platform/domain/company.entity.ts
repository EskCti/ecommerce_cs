import { err, ok, type Result } from '@/shared/result'

export type CompanyData = {
  readonly id: number
  readonly companyId: string
  readonly name: string
  readonly email?: string
  readonly phone?: string
  readonly cpf?: string
  readonly cnpj?: string
  readonly active: boolean
  readonly trial: boolean
  readonly nextBillingDate?: string
  readonly monthlyFee: number
  readonly contracts?: readonly ContractData[]
}

export type ContractData = {
  readonly id: number
  readonly text: string
  readonly signedDate: string
}

export class CompanyEntity implements CompanyData {
  readonly id: number
  readonly companyId: string
  readonly name: string
  readonly email?: string
  readonly phone?: string
  readonly cpf?: string
  readonly cnpj?: string
  readonly active: boolean
  readonly trial: boolean
  readonly nextBillingDate?: string
  readonly monthlyFee: number
  readonly contracts: readonly ContractData[]

  private constructor(data: CompanyData) {
    this.id = data.id
    this.companyId = data.companyId
    this.name = data.name
    this.email = data.email
    this.phone = data.phone
    this.cpf = data.cpf
    this.cnpj = data.cnpj
    this.active = data.active
    this.trial = data.trial
    this.nextBillingDate = data.nextBillingDate
    this.monthlyFee = data.monthlyFee
    this.contracts = data.contracts ?? []
  }

  static fromApi(data: CompanyData): Result<CompanyEntity> {
    if (!data.name?.trim()) return err('Nome da empresa inválido')
    if (!data.id) return err('Id inválido')
    return ok(new CompanyEntity(data))
  }
}
