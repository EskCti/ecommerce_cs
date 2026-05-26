import { err, ok, type Result } from '@/shared/result'

export type CustomerData = {
  readonly id: string
  readonly name: string
  readonly cpf: string
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

export function isValidCpf(value: string): boolean {
  const digits = onlyDigits(value)
  if (digits.length !== 11) return false
  if (/^(\d)\1{10}$/.test(digits)) return false

  let sum = 0
  for (let i = 0; i < 9; i++) sum += Number(digits[i]) * (10 - i)
  let remainder = (sum * 10) % 11
  if (remainder === 10) remainder = 0
  if (remainder !== Number(digits[9])) return false

  sum = 0
  for (let i = 0; i < 10; i++) sum += Number(digits[i]) * (11 - i)
  remainder = (sum * 10) % 11
  if (remainder === 10) remainder = 0
  return remainder === Number(digits[10])
}

export class CustomerEntity implements CustomerData {
  readonly id: string
  readonly name: string
  readonly cpf: string
  readonly phone?: string
  readonly email?: string
  readonly address?: string
  readonly isActive: boolean
  readonly createdAt: string
  readonly updatedAt: string

  private constructor(data: CustomerData) {
    this.id = data.id
    this.name = data.name
    this.cpf = data.cpf
    this.phone = data.phone
    this.email = data.email
    this.address = data.address
    this.isActive = data.isActive
    this.createdAt = data.createdAt
    this.updatedAt = data.updatedAt
  }

  static create(data: CustomerData): Result<CustomerEntity> {
    if (!data.id?.trim()) return err('Id do cliente é obrigatório')
    if (!data.name?.trim()) return err('Nome do cliente é obrigatório')
    if (!isValidCpf(data.cpf)) return err('CPF inválido')
    if (data.email && !data.email.includes('@')) return err('E-mail inválido')
    return ok(new CustomerEntity({ ...data, cpf: onlyDigits(data.cpf) }))
  }

  static fromApi(data: CustomerData): Result<CustomerEntity> {
    return CustomerEntity.create(data)
  }
}
