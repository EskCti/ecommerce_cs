import { err, ok, type Result } from '@/shared/result'

export type ExchangeData = {
  readonly id: string
  readonly customerId: string
  readonly productInId: string
  readonly productOutId: string
  readonly gradeVariantInId?: string
  readonly gradeVariantOutId?: string
  readonly gradeOptionIdsIn?: readonly string[]
  readonly gradeOptionIdsOut?: readonly string[]
  readonly quantity: number
  readonly operatorUserId: string
  readonly registeredAt: string
}

export type ExchangeListItemData = {
  readonly id: string
  readonly customerId: string
  readonly productInId: string
  readonly productOutId: string
  readonly registeredAt: string
}

export const EXCHANGE_FIXED_QUANTITY = 1

export class ExchangeEntity implements ExchangeData {
  readonly id: string
  readonly customerId: string
  readonly productInId: string
  readonly productOutId: string
  readonly gradeVariantInId?: string
  readonly gradeVariantOutId?: string
  readonly gradeOptionIdsIn?: readonly string[]
  readonly gradeOptionIdsOut?: readonly string[]
  readonly quantity: number
  readonly operatorUserId: string
  readonly registeredAt: string

  private constructor(data: ExchangeData) {
    this.id = data.id
    this.customerId = data.customerId
    this.productInId = data.productInId
    this.productOutId = data.productOutId
    this.gradeVariantInId = data.gradeVariantInId
    this.gradeVariantOutId = data.gradeVariantOutId
    this.gradeOptionIdsIn = data.gradeOptionIdsIn
    this.gradeOptionIdsOut = data.gradeOptionIdsOut
    this.quantity = data.quantity
    this.operatorUserId = data.operatorUserId
    this.registeredAt = data.registeredAt
  }

  static create(data: ExchangeData): Result<ExchangeEntity> {
    if (!data.customerId?.trim()) return err('Cliente é obrigatório')
    if (!data.productInId?.trim()) return err('Produto de entrada é obrigatório')
    if (!data.productOutId?.trim()) return err('Produto de saída é obrigatório')
    if (data.productInId === data.productOutId) return err('Produtos de entrada e saída devem ser diferentes')
    if (data.quantity !== EXCHANGE_FIXED_QUANTITY) return err('Quantidade da troca deve ser 1 (RN-050)')
    return ok(new ExchangeEntity(data))
  }

  static fromApi(data: ExchangeData): Result<ExchangeEntity> {
    return ExchangeEntity.create(data)
  }
}

export type RegisterExchangeFormInput = {
  customerId?: string
  cpf?: string
  customerName?: string
  productInId: string
  productOutId: string
  gradeVariantInId?: string
  gradeVariantOutId?: string
  gradeOptionIdsIn?: string[]
  gradeOptionIdsOut?: string[]
}

export function validateRegisterExchangeForm(input: RegisterExchangeFormInput): Result<RegisterExchangeFormInput> {
  if (!input.customerId?.trim() && !input.cpf?.trim()) {
    return err('Informe o cliente ou CPF')
  }
  if (input.cpf?.trim() && !input.customerId?.trim() && !input.customerName?.trim()) {
    return err('Nome do cliente é obrigatório para cadastro por CPF')
  }
  if (!input.productInId?.trim()) return err('Produto de entrada é obrigatório')
  if (!input.productOutId?.trim()) return err('Produto de saída é obrigatório')
  if (input.productInId === input.productOutId) return err('Produtos devem ser diferentes')
  return ok(input)
}
