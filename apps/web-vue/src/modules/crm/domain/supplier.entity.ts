import { err, ok, type Result } from '@/shared/result'
import { isValidCpf } from './customer.entity'

export type PersonType = 'Individual' | 'Company'

export type SupplierData = {
  readonly id: string
  readonly name: string
  readonly personType: PersonType
  readonly taxDocument: string
  readonly phone?: string
  readonly email?: string
  readonly address?: string
  readonly isActive: boolean
  readonly createdAt: string
  readonly updatedAt: string
}

function onlyDigits(value: string): string {
  return value.replace(/\D/g, '')
}

export function isValidCnpj(value: string): boolean {
  const digits = onlyDigits(value)
  if (digits.length !== 14) return false
  if (/^(\d)\1{13}$/.test(digits)) return false

  const weights1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]
  const weights2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]

  let sum = 0
  for (let i = 0; i < 12; i++) sum += Number(digits[i]) * weights1[i]
  let remainder = sum % 11
  const digit1 = remainder < 2 ? 0 : 11 - remainder
  if (digit1 !== Number(digits[12])) return false

  sum = 0
  for (let i = 0; i < 13; i++) sum += Number(digits[i]) * weights2[i]
  remainder = sum % 11
  const digit2 = remainder < 2 ? 0 : 11 - remainder
  return digit2 === Number(digits[13])
}

export class SupplierEntity implements SupplierData {
  readonly id: string
  readonly name: string
  readonly personType: PersonType
  readonly taxDocument: string
  readonly phone?: string
  readonly email?: string
  readonly address?: string
  readonly isActive: boolean
  readonly createdAt: string
  readonly updatedAt: string

  private constructor(data: SupplierData) {
    this.id = data.id
    this.name = data.name
    this.personType = data.personType
    this.taxDocument = data.taxDocument
    this.phone = data.phone
    this.email = data.email
    this.address = data.address
    this.isActive = data.isActive
    this.createdAt = data.createdAt
    this.updatedAt = data.updatedAt
  }

  static create(data: SupplierData): Result<SupplierEntity> {
    if (!data.id?.trim()) return err('Id do fornecedor é obrigatório')
    if (!data.name?.trim()) return err('Nome do fornecedor é obrigatório')
    if (data.personType !== 'Individual' && data.personType !== 'Company') {
      return err('Tipo de pessoa inválido')
    }
    const document = onlyDigits(data.taxDocument)
    if (data.personType === 'Individual') {
      if (!isValidCpf(document)) return err('CPF inválido')
    } else if (!isValidCnpj(document)) {
      return err('CNPJ inválido')
    }
    if (data.email && !data.email.includes('@')) return err('E-mail inválido')
    return ok(new SupplierEntity({ ...data, taxDocument: document }))
  }

  static fromApi(data: SupplierData): Result<SupplierEntity> {
    return SupplierEntity.create(data)
  }
}
