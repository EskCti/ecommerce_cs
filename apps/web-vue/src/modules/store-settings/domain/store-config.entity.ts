import { err, ok, type Result } from '@/shared/result'

export type StoreConfigData = {
  readonly id: string
  readonly tenantId: number
  readonly name: string
  readonly cnpj?: string
  readonly contacts?: string
  readonly address?: string
  readonly discountType: string
  readonly discountValue: number
  readonly commissionRate: number
  readonly reportFormat: string
  readonly apiToken?: string
  readonly logoPath?: string
  readonly createdAt: string
  readonly updatedAt: string
}

export class StoreConfigEntity implements StoreConfigData {
  readonly id: string
  readonly tenantId: number
  readonly name: string
  readonly cnpj?: string
  readonly contacts?: string
  readonly address?: string
  readonly discountType: string
  readonly discountValue: number
  readonly commissionRate: number
  readonly reportFormat: string
  readonly apiToken?: string
  readonly logoPath?: string
  readonly createdAt: string
  readonly updatedAt: string

  private constructor(data: StoreConfigData) {
    this.id = data.id
    this.tenantId = data.tenantId
    this.name = data.name
    this.cnpj = data.cnpj
    this.contacts = data.contacts
    this.address = data.address
    this.discountType = data.discountType
    this.discountValue = data.discountValue
    this.commissionRate = data.commissionRate
    this.reportFormat = data.reportFormat
    this.apiToken = data.apiToken
    this.logoPath = data.logoPath
    this.createdAt = data.createdAt
    this.updatedAt = data.updatedAt
  }

  static create(data: StoreConfigData): Result<StoreConfigEntity> {
    if (!data.id) return err('Store config ID is required')
    if (!data.tenantId) return err('Tenant ID is required')
    if (!data.name?.trim()) return err('Store name is required')
    if (data.discountValue < 0) return err('Discount value cannot be negative')
    if (data.commissionRate < 0) return err('Commission rate cannot be negative')
    return ok(new StoreConfigEntity(data))
  }

  static fromApiResponse(data: StoreConfigData): Result<StoreConfigEntity> {
    return StoreConfigEntity.create(data)
  }

  update(data: Partial<StoreConfigData>): Result<StoreConfigEntity> {
    return StoreConfigEntity.create({
      id: this.id,
      tenantId: this.tenantId,
      name: data.name ?? this.name,
      cnpj: data.cnpj ?? this.cnpj,
      contacts: data.contacts ?? this.contacts,
      address: data.address ?? this.address,
      discountType: data.discountType ?? this.discountType,
      discountValue: data.discountValue ?? this.discountValue,
      commissionRate: data.commissionRate ?? this.commissionRate,
      reportFormat: data.reportFormat ?? this.reportFormat,
      apiToken: data.apiToken ?? this.apiToken,
      logoPath: data.logoPath ?? this.logoPath,
      createdAt: this.createdAt,
      updatedAt: this.updatedAt,
    })
  }

  toApiRequest() {
    return {
      name: this.name,
      cnpj: this.cnpj || null,
      contacts: this.contacts || null,
      address: this.address || null,
      discountType: this.discountType,
      discountValue: this.discountValue,
      commissionRate: this.commissionRate,
      reportFormat: this.reportFormat,
      apiToken: this.apiToken || null,
      logoPath: this.logoPath || null,
    }
  }
}
